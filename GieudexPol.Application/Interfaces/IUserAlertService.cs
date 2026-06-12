using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserAlertService : IService<UserAlert>
    {
        Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId);
        Task<IReadOnlyList<RateSource>> GetActiveRateSourcesAsync();
        Task CreateUserAlertAsync(UserAlert userAlert);
        Task UpdateUserAlertAsync(UserAlert userAlert);
        Task DeleteUserAlertAsync(int userAlertId);
        Task<bool> AcknowledgeAlertAsync(int userAlertId, int userId);
        Task TriggerAlertAsync(int userAlertId, string message);
    }
}
