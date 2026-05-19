using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionService _transactionService;
        

        public WalletService(IWalletRepository walletRepository, ITransactionService transactionService)
        {
            _walletRepository = walletRepository;
            _transactionService = transactionService;
        }

        /// <summary>
        /// Pobiera aktualne salda użytkownika dla wszystkich walut.
        /// </summary>
        public async Task<IEnumerable<Wallet>> GetAvailableBalancesAsync(int userId)
        {
            return await _walletRepository.GetUserWalletsAsync(userId);
        }

        /// <summary>
        /// Wykonuje transakcję handlową, obciążając portfel źródłowy i kredytując portfel docelowy.
        /// </summary>
        public async Task ExecuteTradeTransactionAsync(int userId, int fromCurrencyId, decimal amountFrom, int toCurrencyId, decimal amountTo)
        {
            // Pobieramy wszystkie portfele użytkownika
            var userWallets = await _walletRepository.GetUserWalletsAsync(userId);

            // Szukamy portfela dla waluty źródłowej
            var fromWallet = userWallets.FirstOrDefault(w => w.CurrencyId == fromCurrencyId);
            if (fromWallet == null)
            {
                throw new InvalidOperationException($"Użytkownik nie posiada portfela dla waluty o ID {fromCurrencyId}");
            }

            // Szukamy portfela dla waluty docelowej
            var toWallet = userWallets.FirstOrDefault(w => w.CurrencyId == toCurrencyId);
            if (toWallet == null)
            {
                throw new InvalidOperationException($"Użytkownik nie posiada portfela dla waluty o ID {toCurrencyId}");
            }

            // 1. Debetowanie przy użyciu ID portfela
            await _walletRepository.DebitWalletBalanceAsync(fromWallet.Id, amountFrom);

            // 2. Kredytowanie przy użyciu ID portfela
            await _walletRepository.CreditWalletBalanceAsync(toWallet.Id, amountTo);

            var transactionTime = DateTime.UtcNow;

            // 3. Rejestracja transakcji sprzedaży waluty źródłowej
            var sellTransaction = new Transaction
            {
                UserId = userId,
                CurrencyId = fromCurrencyId,
                TransactionType = "Sell",
                Amount = amountFrom,
                Price = amountTo / (amountFrom > 0 ? amountFrom : 1), // Wyliczenie umownej ceny/kursu
                Commission = 0,
                Timestamp = transactionTime
            };

            // 4. Rejestracja transakcji kupna waluty docelowej
            var buyTransaction = new Transaction
            {
                UserId = userId,
                CurrencyId = toCurrencyId,
                TransactionType = "Buy",
                Amount = amountTo,
                Price = amountFrom / (amountTo > 0 ? amountTo : 1), // Wyliczenie umownej ceny/kursu
                Commission = 0,
                Timestamp = transactionTime
            };

            // Wywołanie metody dodawania z bazowego IService<Transaction>
            await _transactionService.AddAsync(sellTransaction);
            await _transactionService.AddAsync(buyTransaction);
        }

        public async Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId)
        {
            return await _walletRepository.GetUserWalletsAsync(userId);
        }

        public async Task<Wallet> GetByIdAsync(int id)
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

        /// <summary>
        /// Wpłata środków na portfel użytkownika
        /// </summary>
        public async Task DepositAsync(int userId, int currencyId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota wpłaty musi być większa od zera", nameof(amount));
            }

            // Pobieramy portfele użytkownika
            var userWallets = await _walletRepository.GetUserWalletsAsync(userId);

            // Szukamy portfela dla danej waluty
            var wallet = userWallets.FirstOrDefault(w => w.CurrencyId == currencyId);
            if (wallet == null)
            {
                throw new InvalidOperationException($"Użytkownik nie posiada portfela dla waluty o ID {currencyId}");
            }

            // Wykonywanie wpłaty
            await _walletRepository.CreditWalletBalanceAsync(wallet.Id, amount);

            // Rejestracja transakcji wpłaty
            var transaction = new Transaction
            {
                SenderId = userId,
                ReceiverId = userId, // Wpłata jest od i do tego samego użytkownika (system)
                Amount = amount,
                CurrencyId = currencyId,
                TransactionType = "Deposit",
                AppliedFee = 0,
                Status = "Completed",
                Timestamp = DateTime.UtcNow
            };

            await _transactionService.AddAsync(transaction);
        }

        /// <summary>
        /// Wypłata środków z portfela użytkownika
        /// </summary>
        public async Task WithdrawAsync(int userId, int currencyId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota wypłaty musi być większa od zera", nameof(amount));
            }

            // Pobieramy portfele użytkownika
            var userWallets = await _walletRepository.GetUserWalletsAsync(userId);

            // Szukamy portfela dla danej waluty
            var wallet = userWallets.FirstOrDefault(w => w.CurrencyId == currencyId);
            if (wallet == null)
            {
                throw new InvalidOperationException($"Użytkownik nie posiada portfela dla waluty o ID {currencyId}");
            }

            // Sprawdzenie dostępnych środków
            if (wallet.Balance < amount)
            {
                throw new InvalidOperationException("Niewystarczające środki na koncie");
            }

            // Wykonywanie wypłaty
            await _walletRepository.DebitWalletBalanceAsync(wallet.Id, amount);

            // Rejestracja transakcji wypłaty
            var transaction = new Transaction
            {
                SenderId = userId,
                ReceiverId = userId, // Wypłata jest od i do tego samego użytkownika (system)
                Amount = amount,
                CurrencyId = currencyId,
                TransactionType = "Withdrawal",
                AppliedFee = 0,
                Status = "Completed",
                Timestamp = DateTime.UtcNow
            };

            await _transactionService.AddAsync(transaction);
        }
    }
}
