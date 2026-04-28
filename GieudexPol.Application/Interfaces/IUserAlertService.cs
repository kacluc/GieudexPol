using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserAlertService : IService<UserAlert>
    {
        Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId);
    }
}