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
                SenderId = userId,
                ReceiverId = userId, // Self-transaction for trade logging
                CurrencyId = fromCurrencyId,
                TransactionType = "Sell",
                Amount = amountFrom,
                AppliedFee = 0, // No fee for internal trade logging
                Timestamp = transactionTime
            };

            // 4. Rejestracja transakcji kupna waluty docelowej
            var buyTransaction = new Transaction
            {
                SenderId = userId,
                ReceiverId = userId, // Self-transaction for trade logging
                CurrencyId = toCurrencyId,
                TransactionType = "Buy",
                Amount = amountTo,
                AppliedFee = 0, // No fee for internal trade logging
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

    }
}
