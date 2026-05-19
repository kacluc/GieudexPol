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
        Task<Wallet> GetUserWalletAsync(int userId, int currencyId);
        
        // Dodane metody CRUD do kontraktu repozytorium
        Task<Wallet> GetByIdAsync(int id);
        Task AddAsync(Wallet entity);
        Task UpdateAsync(Wallet entity);
        Task<IEnumerable<Wallet>> GetAllAsync();
        Task DeleteAsync(Wallet entity);

    }
}
