using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IWalletService : IService<Wallet>
    {
        Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId);
    }
}