using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface ISystemAccountService
    {
        Task<User> GetPlatformTreasuryAsync(
            CancellationToken cancellationToken = default);

        Task<Wallet> GetOrCreateWalletAsync(
            int userId,
            int currencyId,
            CancellationToken cancellationToken = default);
    }
}
