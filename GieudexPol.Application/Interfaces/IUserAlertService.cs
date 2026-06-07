using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserAlertService : IService<UserAlert>
    {
        Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId);
        Task CreateUserAlertAsync(UserAlert userAlert);
        Task UpdateUserAlertAsync(UserAlert userAlert);
        Task DeleteUserAlertAsync(int userAlertId);
        Task TriggerAlertAsync(int userAlertId, string message);
    }
}
