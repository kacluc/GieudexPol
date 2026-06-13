using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Services
{
    public class OrderMatchingService : IOrderMatchingService
    {
        private readonly ApplicationDbContext _context;

        public OrderMatchingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TradeExecution>> MatchAsync(
            Order incomingOrder,
            CancellationToken cancellationToken = default)
        {
            var oppositeSide = incomingOrder.Side == OrderSide.Buy
                ? OrderSide.Sell
                : OrderSide.Buy;

            var candidatesQuery = _context.Orders
                .Where(order =>
                    order.TradingPairId == incomingOrder.TradingPairId &&
                    order.UserId != incomingOrder.UserId &&
                    order.Side == oppositeSide &&
                    (order.Status == OrderStatus.Open ||
                     order.Status == OrderStatus.PartiallyFilled) &&
                    order.RemainingAmount > 0);

            candidatesQuery = incomingOrder.Side == OrderSide.Buy
                ? candidatesQuery
                    .Where(order => order.Price <= incomingOrder.Price)
                    .OrderBy(order => order.Price)
                    .ThenBy(order => order.CreatedAt)
                    .ThenBy(order => order.Id)
                : candidatesQuery
                    .Where(order => order.Price >= incomingOrder.Price)
                    .OrderByDescending(order => order.Price)
                    .ThenBy(order => order.CreatedAt)
                    .ThenBy(order => order.Id);

            var candidates = await candidatesQuery.ToListAsync(cancellationToken);
            var executions = new List<TradeExecution>();

            foreach (var restingOrder in candidates)
            {
                if (incomingOrder.RemainingAmount <= 0)
                {
                    break;
                }

                var amount = Math.Min(
                    incomingOrder.RemainingAmount,
                    restingOrder.RemainingAmount);
                var executionPrice = restingOrder.Price;
                var buyOrder = incomingOrder.Side == OrderSide.Buy
                    ? incomingOrder
                    : restingOrder;
                var sellOrder = incomingOrder.Side == OrderSide.Sell
                    ? incomingOrder
                    : restingOrder;
                var execution = new TradeExecution
                {
                    BuyOrder = buyOrder,
                    SellOrder = sellOrder,
                    TradingPairId = incomingOrder.TradingPairId,
                    Price = executionPrice,
                    Amount = amount,
                    ExecutedAt = DateTime.UtcNow
                };

                await ExecuteTradeAsync(
                    buyOrder,
                    sellOrder,
                    incomingOrder.TradingPair,
                    execution,
                    cancellationToken);

                await _context.TradeExecutions.AddAsync(execution, cancellationToken);
                executions.Add(execution);
            }

            return executions;
        }

        private async Task ExecuteTradeAsync(
            Order buyOrder,
            Order sellOrder,
            TradingPair pair,
            TradeExecution execution,
            CancellationToken cancellationToken)
        {
            var amount = execution.Amount;
            var buyerQuoteWallet = await GetWalletAsync(
                buyOrder.UserId,
                pair.QuoteCurrencyId,
                cancellationToken);
            var sellerBaseWallet = await GetWalletAsync(
                sellOrder.UserId,
                pair.BaseCurrencyId,
                cancellationToken);
            var buyerBaseWallet = await GetOrCreateWalletAsync(
                buyOrder.UserId,
                pair.BaseCurrencyId,
                cancellationToken);
            var sellerQuoteWallet = await GetOrCreateWalletAsync(
                sellOrder.UserId,
                pair.QuoteCurrencyId,
                cancellationToken);

            var buyReservationBefore = CalculateQuoteAmount(
                buyOrder.RemainingAmount,
                buyOrder.Price);
            buyOrder.RemainingAmount -= amount;
            sellOrder.RemainingAmount -= amount;
            var buyReservationAfter = CalculateQuoteAmount(
                buyOrder.RemainingAmount,
                buyOrder.Price);
            var releasedBuyReservation = buyReservationBefore - buyReservationAfter;
            var quoteAmount = CalculateQuoteAmount(amount, execution.Price);

            buyerQuoteWallet.Release(releasedBuyReservation);
            buyerQuoteWallet.Debit(quoteAmount);
            sellerBaseWallet.DebitReserved(amount);
            buyerBaseWallet.Credit(amount);
            sellerQuoteWallet.Credit(quoteAmount);

            UpdateStatus(buyOrder);
            UpdateStatus(sellOrder);

            await _context.Transactions.AddRangeAsync(
                new Transaction
                {
                    SenderId = sellOrder.UserId,
                    ReceiverId = buyOrder.UserId,
                    CurrencyId = pair.BaseCurrencyId,
                    TransactionType = "OrderBookBuy",
                    Amount = amount,
                    AppliedFee = 0m,
                    Status = "Completed",
                    Timestamp = execution.ExecutedAt,
                    TradeExecution = execution
                },
                new Transaction
                {
                    SenderId = buyOrder.UserId,
                    ReceiverId = sellOrder.UserId,
                    CurrencyId = pair.QuoteCurrencyId,
                    TransactionType = "OrderBookSell",
                    Amount = quoteAmount,
                    AppliedFee = 0m,
                    Status = "Completed",
                    Timestamp = execution.ExecutedAt,
                    TradeExecution = execution
                });
        }

        private async Task<Wallet> GetWalletAsync(
            int userId,
            int currencyId,
            CancellationToken cancellationToken)
        {
            return await FindTrackedOrPersistedWalletAsync(
                    userId,
                    currencyId,
                    cancellationToken)
                ?? throw new InvalidOperationException("Brak portfela z zarezerwowanymi srodkami.");
        }

        private async Task<Wallet> GetOrCreateWalletAsync(
            int userId,
            int currencyId,
            CancellationToken cancellationToken)
        {
            var wallet = await FindTrackedOrPersistedWalletAsync(
                userId,
                currencyId,
                cancellationToken);
            if (wallet != null)
            {
                return wallet;
            }

            wallet = new Wallet
            {
                UserId = userId,
                CurrencyId = currencyId,
                Balance = 0m,
                ReservedBalance = 0m
            };
            await _context.Wallets.AddAsync(wallet, cancellationToken);
            return wallet;
        }

        private async Task<Wallet?> FindTrackedOrPersistedWalletAsync(
            int userId,
            int currencyId,
            CancellationToken cancellationToken)
        {
            var trackedWallet = _context.Wallets.Local.FirstOrDefault(wallet =>
                wallet.UserId == userId && wallet.CurrencyId == currencyId);

            return trackedWallet ?? await _context.Wallets.SingleOrDefaultAsync(
                wallet => wallet.UserId == userId && wallet.CurrencyId == currencyId,
                cancellationToken);
        }

        private static decimal CalculateQuoteAmount(decimal amount, decimal price)
        {
            return decimal.Round(amount * price, 4, MidpointRounding.AwayFromZero);
        }

        private static void UpdateStatus(Order order)
        {
            if (order.RemainingAmount == 0)
            {
                order.Status = OrderStatus.Filled;
                order.ClosedAt = DateTime.UtcNow;
                return;
            }

            order.Status = order.RemainingAmount < order.OriginalAmount
                ? OrderStatus.PartiallyFilled
                : OrderStatus.Open;
        }
    }
}
