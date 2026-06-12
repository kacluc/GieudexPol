using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrencyService _currencyService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ITransactionFeeCalculator _transactionFeeCalculator;

        public WalletService(
            IWalletRepository walletRepository,
            ICurrencyService currencyService,
            IExchangeRateService exchangeRateService,
            ITransactionFeeCalculator transactionFeeCalculator)
        {
            _walletRepository = walletRepository;
            _currencyService = currencyService;
            _exchangeRateService = exchangeRateService;
            _transactionFeeCalculator = transactionFeeCalculator;
        }

        public async Task<IEnumerable<Wallet>> GetAvailableBalancesAsync(int userId)
        {
            return await _walletRepository.GetUserWalletsAsync(userId);
        }

        public async Task<TradeExecutionResultDto> ExecuteTradeTransactionAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken = default)
        {
            if (amountFrom <= 0)
            {
                throw new ArgumentException("Kwota wymiany musi byc wieksza od zera.", nameof(amountFrom));
            }

            if (fromCurrencyId == toCurrencyId)
            {
                throw new InvalidOperationException("Waluta zrodlowa i docelowa musza byc rozne.");
            }

            var userWallets = (await _walletRepository.GetUserWalletsAsync(userId)).ToList();
            var fromWallet = userWallets.FirstOrDefault(wallet => wallet.CurrencyId == fromCurrencyId)
                ?? throw new InvalidOperationException($"Uzytkownik nie posiada portfela dla waluty o ID {fromCurrencyId}.");
            var toWallet = userWallets.FirstOrDefault(wallet => wallet.CurrencyId == toCurrencyId)
                ?? throw new InvalidOperationException($"Uzytkownik nie posiada portfela dla waluty o ID {toCurrencyId}.");

            EnsureTradingCurrencyIsAllowed(fromWallet.Currency.Symbol);
            EnsureTradingCurrencyIsAllowed(toWallet.Currency.Symbol);

            var operationDate = DateTime.Today;
            var requiredCurrencyIds = new[] { fromWallet, toWallet }
                .Where(wallet => !string.Equals(wallet.Currency.Symbol, TradingCurrencyCatalog.BaseCurrencySymbol, StringComparison.OrdinalIgnoreCase))
                .Select(wallet => wallet.CurrencyId)
                .Distinct()
                .ToArray();

            var rates = await GetRatesForTradeAsync(requiredCurrencyIds, operationDate);
            var calculation = CalculateTargetAmount(fromWallet, toWallet, amountFrom, rates);
            var amountTo = calculation.AmountTo;
            var effectiveDate = rates.Count == 0 ? operationDate : rates[0].EffectiveDate.Date;
            var transactionTime = DateTime.UtcNow;

            var sellTransaction = new Transaction
            {
                SenderId = userId,
                ReceiverId = userId,
                CurrencyId = fromCurrencyId,
                TransactionType = "Sell",
                Amount = amountFrom,
                AppliedFee = 0,
                Status = "Completed",
                Timestamp = transactionTime
            };

            var buyTransaction = new Transaction
            {
                SenderId = userId,
                ReceiverId = userId,
                CurrencyId = toCurrencyId,
                TransactionType = "Buy",
                Amount = amountTo,
                AppliedFee = 0,
                Status = "Completed",
                Timestamp = transactionTime
            };

            await _walletRepository.ExecuteTradeAsync(
                fromWallet,
                amountFrom,
                toWallet,
                amountTo,
                sellTransaction,
                buyTransaction);

            return new TradeExecutionResultDto
            {
                AmountTo = amountTo,
                FromCurrency = fromWallet.Currency.Symbol,
                ToCurrency = toWallet.Currency.Symbol,
                FromRateToPln = calculation.FromRateToPln,
                ToRateToPln = calculation.ToRateToPln,
                SellRateSource = calculation.SellRate?.RateSource.Code ?? TradingCurrencyCatalog.BaseCurrencySymbol,
                BuyRateSource = calculation.BuyRate?.RateSource.Code ?? TradingCurrencyCatalog.BaseCurrencySymbol,
                EffectiveDate = effectiveDate
            };
        }

        public async Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId)
        {
            return await _walletRepository.GetUserWalletsAsync(userId);
        }

        public async Task<IEnumerable<Currency>> GetAvailableWalletCurrenciesAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var tradableCurrencies = await _currencyService.GetTradableCurrenciesAsync();

            var existingCurrencyIds = (await _walletRepository.GetUserWalletsAsync(userId))
                .Select(wallet => wallet.CurrencyId)
                .ToHashSet();

            return tradableCurrencies.Where(currency => !existingCurrencyIds.Contains(currency.Id));
        }

        public async Task<Wallet> AddCurrencyWalletAsync(
            int userId,
            int currencyId,
            CancellationToken cancellationToken = default)
        {
            if (await _walletRepository.GetUserWalletAsync(userId, currencyId) != null)
            {
                throw new InvalidOperationException("Portfel dla wybranej waluty juz istnieje.");
            }

            var allowedCurrencies = await GetAvailableWalletCurrenciesAsync(userId, cancellationToken);
            var currency = allowedCurrencies.FirstOrDefault(item => item.Id == currencyId)
                ?? throw new InvalidOperationException("Wybrana waluta nie ma dostepnych kursow i nie moze zostac dodana do portfela.");

            var wallet = new Wallet
            {
                UserId = userId,
                CurrencyId = currencyId,
                Balance = 0
            };

            await _walletRepository.AddAsync(wallet);
            wallet.Currency = currency;
            return wallet;
        }

        public async Task<Wallet?> GetByIdAsync(int id)
        {
            return await _walletRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Wallet entity)
        {
            await _walletRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(Wallet entity)
        {
            await _walletRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Wallet>> GetAllAsync() => await _walletRepository.GetAllAsync();

        public async Task DeleteAsync(Wallet entity) => await _walletRepository.DeleteAsync(entity);

        public async Task DepositAsync(int userId, int currencyId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota wplaty musi byc wieksza od zera.", nameof(amount));
            }

            var wallet = (await _walletRepository.GetUserWalletsAsync(userId))
                .FirstOrDefault(item => item.CurrencyId == currencyId)
                ?? throw new InvalidOperationException($"Uzytkownik nie posiada portfela dla waluty o ID {currencyId}.");

            var fee = await _transactionFeeCalculator.CalculateAsync("Deposit", currencyId, amount);
            var creditedAmount = amount - fee.FeeAmount;
            if (creditedAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Kwota wplaty musi byc wyzsza od naliczonej prowizji.");
            }

            await _walletRepository.ExecuteBalanceOperationAsync(
                wallet.Id,
                creditedAmount,
                CreateBalanceTransaction(
                    userId,
                    currencyId,
                    amount,
                    "Deposit",
                    fee.FeeAmount,
                    fee.TransactionFeeId));
        }

        public async Task WithdrawAsync(int userId, int currencyId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota wyplaty musi byc wieksza od zera.", nameof(amount));
            }

            var wallet = (await _walletRepository.GetUserWalletsAsync(userId))
                .FirstOrDefault(item => item.CurrencyId == currencyId)
                ?? throw new InvalidOperationException($"Uzytkownik nie posiada portfela dla waluty o ID {currencyId}.");

            var fee = await _transactionFeeCalculator.CalculateAsync("Withdrawal", currencyId, amount);
            var totalDebit = amount + fee.FeeAmount;

            if (wallet.AvailableBalance < totalDebit)
            {
                throw new InvalidOperationException(
                    "Niewystarczajace srodki na wyplate wraz z prowizja.");
            }

            await _walletRepository.ExecuteBalanceOperationAsync(
                wallet.Id,
                -totalDebit,
                CreateBalanceTransaction(
                    userId,
                    currencyId,
                    amount,
                    "Withdrawal",
                    fee.FeeAmount,
                    fee.TransactionFeeId));
        }

        private async Task<IReadOnlyList<ExchangeRate>> GetRatesForTradeAsync(
            IReadOnlyCollection<int> requiredCurrencyIds,
            DateTime operationDate)
        {
            if (requiredCurrencyIds.Count == 0)
            {
                return Array.Empty<ExchangeRate>();
            }

            var oldestAcceptedDate = operationDate.AddDays(-14).Date;
            var rates = await _exchangeRateService.GetTradingRateCandidatesAsync(
                requiredCurrencyIds,
                oldestAcceptedDate,
                operationDate);

            if (requiredCurrencyIds.Any(currencyId => rates.All(rate => rate.CurrencyId != currencyId)))
            {
                throw new InvalidOperationException("Nie znaleziono lokalnego aktualnego wspolnego dnia kursowego dla wybranych walut. Odswiez kursy walut lub uruchom synchronizacje.");
            }

            return rates;
        }

        private static void EnsureTradingCurrencyIsAllowed(string currencySymbol)
        {
            if (!string.Equals(
                    currencySymbol,
                    TradingCurrencyCatalog.BaseCurrencySymbol,
                    StringComparison.OrdinalIgnoreCase) &&
                !TradingCurrencyCatalog.Contains(currencySymbol))
            {
                throw new InvalidOperationException("Wybrana waluta nie jest dostepna na wykresach kursow.");
            }
        }

        private static TradeCalculation CalculateTargetAmount(
            Wallet fromWallet,
            Wallet toWallet,
            decimal amountFrom,
            IReadOnlyList<ExchangeRate> rates)
        {
            var sellRate = string.Equals(fromWallet.Currency.Symbol, TradingCurrencyCatalog.BaseCurrencySymbol, StringComparison.OrdinalIgnoreCase)
                ? null
                : rates
                    .Where(rate => rate.CurrencyId == fromWallet.CurrencyId)
                    .OrderByDescending(rate => rate.BuyPrice)
                    .ThenBy(rate => rate.RateSource.Code)
                    .First();
            var buyRate = string.Equals(toWallet.Currency.Symbol, TradingCurrencyCatalog.BaseCurrencySymbol, StringComparison.OrdinalIgnoreCase)
                ? null
                : rates
                    .Where(rate => rate.CurrencyId == toWallet.CurrencyId)
                    .OrderBy(rate => rate.SellPrice)
                    .ThenBy(rate => rate.RateSource.Code)
                    .First();
            var fromRate = string.Equals(fromWallet.Currency.Symbol, TradingCurrencyCatalog.BaseCurrencySymbol, StringComparison.OrdinalIgnoreCase)
                ? 1m
                : sellRate!.BuyPrice;
            var toRate = string.Equals(toWallet.Currency.Symbol, TradingCurrencyCatalog.BaseCurrencySymbol, StringComparison.OrdinalIgnoreCase)
                ? 1m
                : buyRate!.SellPrice;

            return new TradeCalculation(
                Math.Round(amountFrom * fromRate / toRate, 2, MidpointRounding.AwayFromZero),
                fromRate,
                toRate,
                sellRate,
                buyRate);
        }

        private static Transaction CreateBalanceTransaction(
            int userId,
            int currencyId,
            decimal amount,
            string transactionType,
            decimal appliedFee,
            Guid? transactionFeeId)
        {
            return new Transaction
            {
                SenderId = userId,
                ReceiverId = userId,
                Amount = amount,
                CurrencyId = currencyId,
                TransactionType = transactionType,
                AppliedFee = appliedFee,
                TransactionFeeId = transactionFeeId,
                Status = "Completed",
                Timestamp = DateTime.UtcNow
            };
        }

        private sealed record TradeCalculation(
            decimal AmountTo,
            decimal FromRateToPln,
            decimal ToRateToPln,
            ExchangeRate? SellRate,
            ExchangeRate? BuyRate);
    }
}
