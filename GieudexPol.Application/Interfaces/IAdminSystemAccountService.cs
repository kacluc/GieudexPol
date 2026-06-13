using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface IAdminSystemAccountService
    {
        Task<IReadOnlyList<AdminSystemAccountDto>> GetAccountsAsync(
            CancellationToken cancellationToken = default);
    }
}
