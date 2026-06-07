using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface IWalletRepository
    {
        Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId);
        Task DebitWalletBalanceAsync(int walletId, decimal amount);
        Task CreditWalletBalanceAsync(int walletId, decimal amount);
        Task<Wallet?> GetUserWalletAsync(int userId, int currencyId);
        Task ExecuteTradeAsync(
            Wallet fromWallet,
            decimal amountFrom,
            Wallet toWallet,
            decimal amountTo,
            Transaction sellTransaction,
            Transaction buyTransaction);
        Task ExecuteTransferAsync(
            int senderWalletId,
            int receiverUserId,
            int currencyId,
            decimal amount,
            decimal fee,
            Transaction transaction);
        
        // Dodane metody CRUD do kontraktu repozytorium
        Task<Wallet?> GetByIdAsync(int id);
        Task AddAsync(Wallet entity);
        Task UpdateAsync(Wallet entity);
        Task<IEnumerable<Wallet>> GetAllAsync();
        Task DeleteAsync(Wallet entity);

    }
}
