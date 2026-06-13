using System.Data;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Services
{
    public class InstantExchangeService : IInstantExchangeService
    {
        private const int MaximumRateAgeDays = 7;

        private readonly ApplicationDbContext _context;
        private readonly ITransactionFeeCalculator _feeCalculator;
        private readonly ISystemAccountService _systemAccounts;

        public InstantExchangeService(
            ApplicationDbContext context,
            ITransactionFeeCalculator feeCalculator,
            ISystemAccountService systemAccounts)
        {
            _context = context;
            _feeCalculator = feeCalculator;
            _systemAccounts = systemAccounts;
        }

        public async Task<TradeExecutionResultDto> ExecuteAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken = default)
        {
            if (amountFrom <= 0)
            {
                throw new ArgumentException(
                    "Kwota wymiany musi byc wieksza od zera.",
                    nameof(amountFrom));
            }

            if (fromCurrencyId == toCurrencyId)
            {
                throw new InvalidOperationException(
                    "Waluta zrodlowa i docelowa musza byc rozne.");
            }

            if (!_context.Database.IsRelational())
            {
                return await ExecuteCoreAsync(
                    userId,
                    fromCurrencyId,
                    amountFrom,
                    toCurrencyId,
                    cancellationToken);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var result = await ExecuteCoreAsync(
                    userId,
                    fromCurrencyId,
                    amountFrom,
                    toCurrencyId,
                    cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            });
        }

        public async Task<ExchangePreviewResultDto> PreviewAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken = default)
        {
            var quote = await BuildQuoteAsync(
                userId,
                fromCurrencyId,
                amountFrom,
                toCurrencyId,
                cancellationToken);

            return new ExchangePreviewResultDto
            {
                FromCurrencyCode = quote.FromCurrency.Symbol,
                ToCurrencyCode = quote.ToCurrency.Symbol,
                InputAmount = amountFrom,
                EstimatedOutputAmount = quote.Offer.AmountTo,
                Rate = quote.Offer.AppliedRate,
                FeeAmount = quote.Fee.FeeAmount,
                FeeCurrencyCode = quote.FromCurrency.Symbol,
                TotalDebitAmount = quote.TotalDebit,
                RateDate = quote.Offer.EffectiveDate,
                RateSourceCode = quote.Offer.RateSourceCode,
                RateSourceName = quote.Offer.RateSourceName,
                HasSufficientFunds = quote.HasSufficientFunds,
                IsPreview = true,
                Message = "To jest tylko symulacja. Salda i historia transakcji nie zostaly zmienione."
            };
        }

        private async Task<TradeExecutionResultDto> ExecuteCoreAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken)
        {
            var quote = await BuildQuoteAsync(
                userId,
                fromCurrencyId,
                amountFrom,
                toCurrencyId,
                cancellationToken);
            var fromCurrency = quote.FromCurrency;
            var toCurrency = quote.ToCurrency;
            var selected = quote.Offer;

            var userFromWallet = await GetWalletAsync(
                userId,
                fromCurrencyId,
                cancellationToken);
            var userToWallet = await _systemAccounts.GetOrCreateWalletAsync(
                userId,
                toCurrencyId,
                cancellationToken);
            if (userFromWallet.AvailableBalance < quote.TotalDebit)
            {
                throw new InvalidOperationException(
                    "Niewystarczajace srodki na wymiane wraz z prowizja.");
            }

            var sourceFromWallet = await _systemAccounts.GetOrCreateWalletAsync(
                selected.SystemUserId,
                fromCurrencyId,
                cancellationToken);
            var sourceToWallet = await _systemAccounts.GetOrCreateWalletAsync(
                selected.SystemUserId,
                toCurrencyId,
                cancellationToken);

            userFromWallet.Debit(quote.TotalDebit);
            userToWallet.Credit(selected.AmountTo);
            sourceFromWallet.Credit(amountFrom);
            sourceToWallet.Debit(selected.AmountTo);

            var treasury = await _systemAccounts.GetPlatformTreasuryAsync(
                cancellationToken);
            var treasuryWallet = await _systemAccounts.GetOrCreateWalletAsync(
                treasury.Id,
                fromCurrencyId,
                cancellationToken);
            treasuryWallet.Credit(quote.Fee.FeeAmount);

            var executedAt = DateTime.UtcNow;
            var execution = new ExchangeExecution
            {
                UserId = userId,
                RateSourceId = selected.RateSourceId,
                FromCurrencyId = fromCurrencyId,
                ToCurrencyId = toCurrencyId,
                FromAmount = amountFrom,
                ToAmount = selected.AmountTo,
                Rate = selected.AppliedRate,
                FeeAmount = quote.Fee.FeeAmount,
                FeeCurrencyId = fromCurrencyId,
                ExecutedAt = executedAt
            };
            await _context.ExchangeExecutions.AddAsync(execution, cancellationToken);
            await _context.Transactions.AddRangeAsync(
                new Transaction
                {
                    SenderId = userId,
                    ReceiverId = selected.SystemUserId,
                    CurrencyId = fromCurrencyId,
                    TransactionType = "InstantExchangeSell",
                    Amount = amountFrom,
                    AppliedFee = quote.Fee.FeeAmount,
                    TransactionFeeId = quote.Fee.TransactionFeeId,
                    Status = "Completed",
                    Timestamp = executedAt,
                    ExchangeExecution = execution
                },
                new Transaction
                {
                    SenderId = selected.SystemUserId,
                    ReceiverId = userId,
                    CurrencyId = toCurrencyId,
                    TransactionType = "InstantExchangeBuy",
                    Amount = selected.AmountTo,
                    AppliedFee = 0m,
                    Status = "Completed",
                    Timestamp = executedAt,
                    ExchangeExecution = execution
                });

            await _context.SaveChangesAsync(cancellationToken);

            return new TradeExecutionResultDto
            {
                AmountTo = selected.AmountTo,
                FromCurrency = fromCurrency.Symbol,
                ToCurrency = toCurrency.Symbol,
                FromRateToPln = selected.FromRateToPln,
                ToRateToPln = selected.ToRateToPln,
                SellRateSource = selected.RateSourceCode,
                BuyRateSource = selected.RateSourceCode,
                RateSource = selected.RateSourceCode,
                EffectiveDate = selected.EffectiveDate,
                AppliedRate = selected.AppliedRate,
                FeeAmount = quote.Fee.FeeAmount,
                FeeCurrency = fromCurrency.Symbol,
                ExchangeExecutionId = execution.Id
            };
        }

        private async Task<ExchangeQuote> BuildQuoteAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken)
        {
            ValidateRequest(fromCurrencyId, amountFrom, toCurrencyId);
            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken)
                ?? throw new InvalidOperationException("Uzytkownik nie istnieje.");
            if (user.AccountType is AccountType.RateSourceSystem or
                AccountType.PlatformTreasury)
            {
                throw new InvalidOperationException(
                    "Konta systemowe nie korzystaja z szybkiej wymiany uzytkownika.");
            }

            var currencies = await _context.Currencies
                .AsNoTracking()
                .Where(currency =>
                    currency.Id == fromCurrencyId ||
                    currency.Id == toCurrencyId)
                .ToListAsync(cancellationToken);
            var fromCurrency = currencies.SingleOrDefault(
                                   currency => currency.Id == fromCurrencyId)
                               ?? throw new InvalidOperationException(
                                   "Waluta zrodlowa nie istnieje.");
            var toCurrency = currencies.SingleOrDefault(
                                 currency => currency.Id == toCurrencyId)
                             ?? throw new InvalidOperationException(
                                 "Waluta docelowa nie istnieje.");
            var fee = await _feeCalculator.CalculateAsync(
                "InstantExchange",
                fromCurrencyId,
                amountFrom,
                cancellationToken);
            var totalDebit = amountFrom + fee.FeeAmount;
            var offer = await FindBestOfferAsync(
                user,
                fromCurrency,
                toCurrency,
                amountFrom,
                cancellationToken);
            var availableBalance = await _context.Wallets
                .AsNoTracking()
                .Where(wallet =>
                    wallet.UserId == userId &&
                    wallet.CurrencyId == fromCurrencyId)
                .Select(wallet => (decimal?)wallet.Balance - wallet.ReservedBalance)
                .SingleOrDefaultAsync(cancellationToken);

            return new ExchangeQuote(
                fromCurrency,
                toCurrency,
                fee,
                totalDebit,
                availableBalance.HasValue &&
                availableBalance.Value >= totalDebit,
                offer);
        }

        private async Task<ExchangeOffer> FindBestOfferAsync(
            User user,
            Currency fromCurrency,
            Currency toCurrency,
            decimal amountFrom,
            CancellationToken cancellationToken)
        {
            var oldestDate = DateTime.Today.AddDays(-MaximumRateAgeDays);
            var requiredCurrencyIds = new[] { fromCurrency, toCurrency }
                .Where(currency =>
                    !string.Equals(
                        currency.Symbol,
                        TradingCurrencyCatalog.BaseCurrencySymbol,
                        StringComparison.OrdinalIgnoreCase))
                .Select(currency => currency.Id)
                .Distinct()
                .ToList();

            var rates = await _context.ExchangeRates
                .AsNoTracking()
                .Include(rate => rate.RateSource)
                    .ThenInclude(source => source.SystemUser)
                .Where(rate =>
                    requiredCurrencyIds.Contains(rate.CurrencyId) &&
                    rate.EffectiveDate >= oldestDate &&
                    rate.EffectiveDate <= DateTime.Today &&
                    rate.RateSource.IsActive &&
                    rate.RateSource.SystemUserId.HasValue &&
                    rate.RateSource.SystemUser!.AccountType ==
                    AccountType.RateSourceSystem)
                .ToListAsync(cancellationToken);

            if (user.AccountType is not AccountType.AdminUser and
                not AccountType.SuperAdminUser)
            {
                rates = rates.Where(rate =>
                    rate.RateSource.Code != DevelopmentIdentity.RateSourceCode &&
                    rate.RateSource.Code != DevelopmentIdentity.RateSourceCodeB)
                    .ToList();
            }

            var latestBySourceAndCurrency = rates
                .GroupBy(rate => new { rate.RateSourceId, rate.CurrencyId })
                .Select(group => group
                    .OrderByDescending(rate => rate.EffectiveDate)
                    .ThenByDescending(rate => rate.FetchedAt)
                    .First())
                .ToList();
            var candidates = new List<ExchangeOffer>();
            var sourceUserIds = latestBySourceAndCurrency
                .Select(rate => rate.RateSource.SystemUserId!.Value)
                .Distinct()
                .ToList();
            var targetLiquidity = await _context.Wallets
                .AsNoTracking()
                .Where(wallet =>
                    sourceUserIds.Contains(wallet.UserId) &&
                    wallet.CurrencyId == toCurrency.Id)
                .Select(wallet => new
                {
                    wallet.UserId,
                    AvailableBalance = wallet.Balance - wallet.ReservedBalance
                })
                .ToDictionaryAsync(
                    wallet => wallet.UserId,
                    wallet => wallet.AvailableBalance,
                    cancellationToken);

            foreach (var sourceRates in latestBySourceAndCurrency
                         .GroupBy(rate => rate.RateSourceId))
            {
                var sourceRate = string.Equals(
                    fromCurrency.Symbol,
                    TradingCurrencyCatalog.BaseCurrencySymbol,
                    StringComparison.OrdinalIgnoreCase)
                    ? null
                    : sourceRates.SingleOrDefault(
                        rate => rate.CurrencyId == fromCurrency.Id);
                var targetRate = string.Equals(
                    toCurrency.Symbol,
                    TradingCurrencyCatalog.BaseCurrencySymbol,
                    StringComparison.OrdinalIgnoreCase)
                    ? null
                    : sourceRates.SingleOrDefault(
                        rate => rate.CurrencyId == toCurrency.Id);
                if ((sourceRate == null &&
                     fromCurrency.Symbol != TradingCurrencyCatalog.BaseCurrencySymbol) ||
                    (targetRate == null &&
                     toCurrency.Symbol != TradingCurrencyCatalog.BaseCurrencySymbol))
                {
                    continue;
                }

                var rateSource = (sourceRate ?? targetRate)!.RateSource;
                var fromRateToPln = sourceRate?.BuyPrice ?? 1m;
                var toRateToPln = targetRate?.SellPrice ?? 1m;
                if (fromRateToPln <= 0 || toRateToPln <= 0)
                {
                    continue;
                }

                var amountTo = decimal.Round(
                    amountFrom * fromRateToPln / toRateToPln,
                    4,
                    MidpointRounding.AwayFromZero);
                var systemUserId = rateSource.SystemUserId!.Value;
                var effectiveDate = new[] {
                        sourceRate?.EffectiveDate,
                        targetRate?.EffectiveDate
                    }
                    .Where(date => date.HasValue)
                    .Select(date => date!.Value)
                    .DefaultIfEmpty(DateTime.Today)
                    .Min();

                candidates.Add(new ExchangeOffer(
                    rateSource.Id,
                    systemUserId,
                    rateSource.Code,
                    rateSource.Name,
                    amountTo,
                    amountTo / amountFrom,
                    fromRateToPln,
                    toRateToPln,
                    effectiveDate,
                    targetLiquidity.GetValueOrDefault(systemUserId)));
            }

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException(
                    "Brak kursu z ostatnich 7 dni dla wybranej wymiany.");
            }

            return candidates
                .Where(candidate =>
                    candidate.AvailableTargetLiquidity >= candidate.AmountTo)
                .OrderByDescending(candidate => candidate.AmountTo)
                .ThenByDescending(candidate => candidate.EffectiveDate)
                .ThenBy(candidate => candidate.RateSourceCode)
                .FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "Brak zrodla z aktualnym kursem i wystarczajaca plynnoscia.");
        }

        private static void ValidateRequest(
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId)
        {
            if (amountFrom <= 0)
            {
                throw new ArgumentException(
                    "Kwota wymiany musi byc wieksza od zera.",
                    nameof(amountFrom));
            }

            if (fromCurrencyId == toCurrencyId)
            {
                throw new InvalidOperationException(
                    "Waluta zrodlowa i docelowa musza byc rozne.");
            }
        }

        private async Task<Wallet> GetWalletAsync(
            int userId,
            int currencyId,
            CancellationToken cancellationToken)
        {
            return await _context.Wallets.SingleOrDefaultAsync(
                       wallet =>
                           wallet.UserId == userId &&
                           wallet.CurrencyId == currencyId,
                       cancellationToken)
                   ?? throw new InvalidOperationException(
                       "Brak portfela dla waluty zrodlowej.");
        }

        private sealed record ExchangeOffer(
            int RateSourceId,
            int SystemUserId,
            string RateSourceCode,
            string RateSourceName,
            decimal AmountTo,
            decimal AppliedRate,
            decimal FromRateToPln,
            decimal ToRateToPln,
            DateTime EffectiveDate,
            decimal AvailableTargetLiquidity);

        private sealed record ExchangeQuote(
            Currency FromCurrency,
            Currency ToCurrency,
            OperationFeeCalculationDto Fee,
            decimal TotalDebit,
            bool HasSufficientFunds,
            ExchangeOffer Offer);
    }
}
