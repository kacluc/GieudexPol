using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface IWalletService
    {
        Task<IEnumerable<Wallet>> GetAvailableBalancesAsync(int userId);
        Task ExecuteTradeTransactionAsync(int userId, int fromCurrencyId, decimal amountFrom, int toCurrencyId, decimal amountTo);
        Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId);

        // Dodane metody CRUD do kontraktu usługi
        Task<Wallet> GetByIdAsync(int id);
        Task AddAsync(Wallet entity);
        Task UpdateAsync(Wallet entity);

        Task<IEnumerable<Wallet>> GetAllAsync();
        Task DeleteAsync(Wallet entity);

        // Nowe metody dla wpłat i wypłat
        Task DepositAsync(int userId, int currencyId, decimal amount);
        Task WithdrawAsync(int userId, int currencyId, decimal amount);
    }
}
