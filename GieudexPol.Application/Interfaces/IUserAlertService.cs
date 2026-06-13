using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserAlertService : IService<UserAlert>
    {
        Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId);
        Task<IReadOnlyList<RateSource>> GetActiveRateSourcesAsync();
        Task<IReadOnlyList<RateSource>> GetActiveRateSourcesAsync(bool includeTestSources);
        Task CreateUserAlertAsync(UserAlert userAlert);
        Task CreateUserAlertAsync(UserAlert userAlert, bool allowTestRateSources);
        Task UpdateUserAlertAsync(UserAlert userAlert);
        Task UpdateUserAlertAsync(UserAlert userAlert, bool allowTestRateSources);
        Task DeleteUserAlertAsync(int userAlertId);
    }
}
