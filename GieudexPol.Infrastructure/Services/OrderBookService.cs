using System.Data;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GieudexPol.Infrastructure.Services
{
    public class OrderBookService : IOrderBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderMatchingService _matchingService;
        private readonly ITradingAlertEvaluationService? _tradingAlertEvaluationService;
        private readonly ILogger<OrderBookService>? _logger;
        private readonly ITransactionFeeCalculator _feeCalculator;

        public OrderBookService(
            ApplicationDbContext context,
            IOrderMatchingService matchingService,
            ITransactionFeeCalculator feeCalculator,
            ITradingAlertEvaluationService? tradingAlertEvaluationService = null,
            ILogger<OrderBookService>? logger = null)
        {
            _context = context;
            _matchingService = matchingService;
            _feeCalculator = feeCalculator;
            _tradingAlertEvaluationService = tradingAlertEvaluationService;
            _logger = logger;
        }

        public async Task<OrderDto> PlaceOrderAsync(
            int userId,
            CreateOrderRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);
            var result = await ExecuteAtomicallyAsync(
                () => PlaceOrderCoreAsync(userId, request, cancellationToken),
                cancellationToken);

            if (_tradingAlertEvaluationService != null)
            {
                try
                {
                    var pairId = await _context.Orders
                        .AsNoTracking()
                        .Where(order => order.Id == result.Id)
                        .Select(order => order.TradingPairId)
                        .SingleAsync(cancellationToken);
                    await _tradingAlertEvaluationService.EvaluatePairAsync(
                        pairId,
                        cancellationToken);
                }
                catch (Exception exception) when (
                    exception is not OperationCanceledException ||
                    !cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogError(
                        exception,
                        "Nie udalo sie ocenic alertow rynku po zleceniu {OrderId}.",
                        result.Id);
                }
            }

            return result;
        }

        public async Task<OrderDto> PlaceRateSourceOrderAsync(
            string rateSourceCode,
            CreateOrderRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rateSourceCode))
            {
                throw new ArgumentException("Kod zrodla kursu jest wymagany.");
            }

            var source = await _context.RateSources
                .Include(item => item.SystemUser)
                .SingleOrDefaultAsync(item =>
                    item.Code == rateSourceCode.Trim().ToUpper() &&
                    item.IsActive,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Aktywne zrodlo kursu nie istnieje.");

            if (!source.SystemUserId.HasValue ||
                source.SystemUser?.AccountType != AccountType.RateSourceSystem)
            {
                throw new InvalidOperationException(
                    "Zrodlo kursu nie ma skonfigurowanego konta systemowego.");
            }

            return await PlaceOrderAsync(
                source.SystemUserId.Value,
                request,
                cancellationToken);
        }

        public async Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(order => order.TradingPair)
                    .ThenInclude(pair => pair.BaseCurrency)
                .Include(order => order.TradingPair)
                    .ThenInclude(pair => pair.QuoteCurrency)
                .Where(order => order.UserId == userId)
                .OrderByDescending(order => order.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(MapOrder).ToList();
        }

        public Task CancelOrderAsync(
            int userId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            return ExecuteAtomicallyAsync(
                async () =>
                {
                    var order = await _context.Orders
                        .Include(item => item.TradingPair)
                        .SingleOrDefaultAsync(item => item.Id == orderId, cancellationToken);

                    if (order == null || order.UserId != userId)
                    {
                        throw new KeyNotFoundException("Zlecenie nie istnieje.");
                    }

                    if (order.Status != OrderStatus.Open &&
                        order.Status != OrderStatus.PartiallyFilled)
                    {
                        throw new InvalidOperationException("Mozna anulowac tylko aktywne zlecenie.");
                    }

                    var currencyId = order.Side == OrderSide.Buy
                        ? order.TradingPair.QuoteCurrencyId
                        : order.TradingPair.BaseCurrencyId;
                    var wallet = await _context.Wallets.SingleAsync(
                        item => item.UserId == userId && item.CurrencyId == currencyId,
                        cancellationToken);
                    var reservedAmount = order.Side == OrderSide.Buy
                        ? await CalculateRemainingBuyReservationAsync(
                            order,
                            cancellationToken)
                        : order.RemainingAmount;

                    wallet.Release(reservedAmount);
                    order.Status = OrderStatus.Cancelled;
                    order.ClosedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                },
                cancellationToken);
        }

        public async Task<OrderBookDto> GetOrderBookAsync(
            string baseCurrencyCode,
            string quoteCurrencyCode,
            int depth,
            CancellationToken cancellationToken = default)
        {
            var pair = await FindPairAsync(
                baseCurrencyCode,
                quoteCurrencyCode,
                cancellationToken);
            var effectiveDepth = Math.Clamp(depth, 1, 100);
            var activeOrders = await _context.Orders
                .AsNoTracking()
                .Where(order =>
                    order.TradingPairId == pair.Id &&
                    (order.Status == OrderStatus.Open ||
                     order.Status == OrderStatus.PartiallyFilled) &&
                    order.RemainingAmount > 0)
                .Select(order => new BookOrderSource(
                    order.Side,
                    order.Price,
                    order.RemainingAmount))
                .ToListAsync(cancellationToken);

            return new OrderBookDto
            {
                Pair = FormatPair(pair),
                BaseCurrency = pair.BaseCurrency.Symbol,
                QuoteCurrency = pair.QuoteCurrency.Symbol,
                BuyOrders = AggregateLevels(
                    activeOrders.Where(order => order.Side == OrderSide.Buy),
                    descending: true,
                    effectiveDepth),
                SellOrders = AggregateLevels(
                    activeOrders.Where(order => order.Side == OrderSide.Sell),
                    descending: false,
                    effectiveDepth)
            };
        }

        public async Task<IReadOnlyList<TradingPairDto>> GetTradingPairsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.TradingPairs
                .AsNoTracking()
                .Include(pair => pair.BaseCurrency)
                .Include(pair => pair.QuoteCurrency)
                .Where(pair => pair.IsActive)
                .OrderBy(pair => pair.BaseCurrency.Symbol)
                .ThenBy(pair => pair.QuoteCurrency.Symbol)
                .Select(pair => new TradingPairDto
                {
                    Id = pair.Id,
                    Pair = pair.BaseCurrency.Symbol + "/" + pair.QuoteCurrency.Symbol,
                    BaseCurrency = pair.BaseCurrency.Symbol,
                    QuoteCurrency = pair.QuoteCurrency.Symbol,
                    TickSize = pair.TickSize,
                    IsActive = pair.IsActive
                })
                .ToListAsync(cancellationToken);
        }

        private async Task<OrderDto> PlaceOrderCoreAsync(
            int userId,
            CreateOrderRequestDto request,
            CancellationToken cancellationToken)
        {
            var pair = await FindPairAsync(
                request.BaseCurrencyCode,
                request.QuoteCurrencyCode,
                cancellationToken);

            if (pair.TickSize <= 0)
            {
                throw new InvalidOperationException("Para walutowa ma nieprawidlowy krok ceny.");
            }

            if (request.Price % pair.TickSize != 0)
            {
                throw new ArgumentException(
                    $"Cena musi byc wielokrotnoscia kroku {pair.TickSize:0.####}.");
            }

            var reservedCurrencyId = request.Side == OrderSide.Buy
                ? pair.QuoteCurrencyId
                : pair.BaseCurrencyId;
            var reservedAmount = request.Side == OrderSide.Buy
                ? await CalculateInitialBuyReservationAsync(
                    request.Amount,
                    request.Price,
                    pair.QuoteCurrencyId,
                    cancellationToken)
                : request.Amount;
            var wallet = await _context.Wallets.SingleOrDefaultAsync(
                item => item.UserId == userId && item.CurrencyId == reservedCurrencyId,
                cancellationToken)
                ?? throw new InvalidOperationException("Brak portfela dla waluty wymaganej przez zlecenie.");

            wallet.Reserve(reservedAmount);

            var order = new Order
            {
                UserId = userId,
                TradingPair = pair,
                Side = request.Side,
                Type = OrderType.Limit,
                Status = OrderStatus.Open,
                Price = request.Price,
                OriginalAmount = request.Amount,
                RemainingAmount = request.Amount,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await _matchingService.MatchAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return MapOrder(order);
        }

        private async Task<TradingPair> FindPairAsync(
            string baseCurrencyCode,
            string quoteCurrencyCode,
            CancellationToken cancellationToken)
        {
            var baseCode = baseCurrencyCode.Trim().ToUpperInvariant();
            var quoteCode = quoteCurrencyCode.Trim().ToUpperInvariant();
            var pair = await _context.TradingPairs
                .Include(item => item.BaseCurrency)
                .Include(item => item.QuoteCurrency)
                .SingleOrDefaultAsync(item =>
                    item.IsActive &&
                    item.BaseCurrency.Symbol == baseCode &&
                    item.QuoteCurrency.Symbol == quoteCode,
                    cancellationToken);

            return pair ?? throw new KeyNotFoundException(
                $"Aktywna para {baseCode}/{quoteCode} nie istnieje.");
        }

        private async Task<T> ExecuteAtomicallyAsync<T>(
            Func<Task<T>> operation,
            CancellationToken cancellationToken)
        {
            if (!_context.Database.IsRelational())
            {
                return await operation();
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var result = await operation();
                await transaction.CommitAsync(cancellationToken);
                return result;
            });
        }

        private async Task ExecuteAtomicallyAsync(
            Func<Task> operation,
            CancellationToken cancellationToken)
        {
            await ExecuteAtomicallyAsync(
                async () =>
                {
                    await operation();
                    return true;
                },
                cancellationToken);
        }

        private static IReadOnlyList<OrderBookLevelDto> AggregateLevels(
            IEnumerable<BookOrderSource> orders,
            bool descending,
            int depth)
        {
            var grouped = orders
                .GroupBy(order => order.Price)
                .Select(group => new
                {
                    Price = group.Key,
                    Amount = group.Sum(order => order.RemainingAmount),
                    OrdersCount = group.Count()
                });
            var sorted = descending
                ? grouped.OrderByDescending(level => level.Price)
                : grouped.OrderBy(level => level.Price);
            var total = 0m;

            return sorted
                .Take(depth)
                .Select(level =>
                {
                    total += level.Amount;
                    return new OrderBookLevelDto
                    {
                        Price = level.Price,
                        Amount = level.Amount,
                        Total = total,
                        OrdersCount = level.OrdersCount
                    };
                })
                .ToList();
        }

        private static void ValidateRequest(CreateOrderRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.BaseCurrencyCode) ||
                string.IsNullOrWhiteSpace(request.QuoteCurrencyCode))
            {
                throw new ArgumentException("Nalezy podac obie waluty pary.");
            }

            if (string.Equals(
                request.BaseCurrencyCode.Trim(),
                request.QuoteCurrencyCode.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Waluta bazowa i kwotowana musza byc rozne.");
            }

            if (!Enum.IsDefined(request.Side))
            {
                throw new ArgumentException("Strona zlecenia musi byc Buy albo Sell.");
            }

            if (request.Price <= 0 || request.Amount <= 0)
            {
                throw new ArgumentException("Cena i ilosc musza byc wieksze od zera.");
            }

            if (decimal.Round(request.Amount, 4) != request.Amount)
            {
                throw new ArgumentException("Ilosc moze miec maksymalnie 4 miejsca po przecinku.");
            }
        }

        private static decimal CalculateQuoteAmount(decimal amount, decimal price)
        {
            return decimal.Round(amount * price, 4, MidpointRounding.AwayFromZero);
        }

        private async Task<decimal> CalculateInitialBuyReservationAsync(
            decimal amount,
            decimal price,
            int quoteCurrencyId,
            CancellationToken cancellationToken)
        {
            var quoteAmount = CalculateQuoteAmount(amount, price);
            var fee = await _feeCalculator.CalculateAsync(
                "OrderBook",
                quoteCurrencyId,
                quoteAmount,
                cancellationToken);
            return quoteAmount + fee.FeeAmount;
        }

        private async Task<decimal> CalculateRemainingBuyReservationAsync(
            Order order,
            CancellationToken cancellationToken)
        {
            if (order.RemainingAmount <= 0)
            {
                return 0m;
            }

            var remainingQuote = CalculateQuoteAmount(
                order.RemainingAmount,
                order.Price);
            var projectedQuote = order.ExecutedQuoteAmount + remainingQuote;
            var projectedFee = await _feeCalculator.CalculateAsync(
                "OrderBook",
                order.TradingPair.QuoteCurrencyId,
                projectedQuote,
                cancellationToken);
            return remainingQuote + Math.Max(
                0m,
                projectedFee.FeeAmount - order.FeePaid);
        }

        private static string FormatPair(TradingPair pair)
        {
            return pair.BaseCurrency.Symbol + "/" + pair.QuoteCurrency.Symbol;
        }

        private static OrderDto MapOrder(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                Pair = FormatPair(order.TradingPair),
                BaseCurrency = order.TradingPair.BaseCurrency.Symbol,
                QuoteCurrency = order.TradingPair.QuoteCurrency.Symbol,
                Side = order.Side,
                Type = order.Type,
                Status = order.Status,
                Price = order.Price,
                OriginalAmount = order.OriginalAmount,
                RemainingAmount = order.RemainingAmount,
                CreatedAt = order.CreatedAt,
                ClosedAt = order.ClosedAt
            };
        }

        private sealed record BookOrderSource(
            OrderSide Side,
            decimal Price,
            decimal RemainingAmount);
    }
}
