using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IWalletRepository : IRepository<Wallet>
    {
        Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId);
    }
}