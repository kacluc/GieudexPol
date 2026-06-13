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
        private readonly ITransactionFeeCalculator _transactionFeeCalculator;
        private readonly IInstantExchangeService _instantExchangeService;

        public WalletService(
            IWalletRepository walletRepository,
            ICurrencyService currencyService,
            ITransactionFeeCalculator transactionFeeCalculator,
            IInstantExchangeService instantExchangeService)
        {
            _walletRepository = walletRepository;
            _currencyService = currencyService;
            _transactionFeeCalculator = transactionFeeCalculator;
            _instantExchangeService = instantExchangeService;
        }

        public async Task<IEnumerable<Wallet>> GetAvailableBalancesAsync(int userId)
        {
            return await _walletRepository.GetUserWalletsAsync(userId);
        }

        public Task<TradeExecutionResultDto> ExecuteTradeTransactionAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken = default)
        {
            return _instantExchangeService.ExecuteAsync(
                userId,
                fromCurrencyId,
                amountFrom,
                toCurrencyId,
                cancellationToken);
        }

        public Task<ExchangePreviewResultDto> PreviewTradeAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken = default)
        {
            return _instantExchangeService.PreviewAsync(
                userId,
                fromCurrencyId,
                amountFrom,
                toCurrencyId,
                cancellationToken);
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

    }
}
