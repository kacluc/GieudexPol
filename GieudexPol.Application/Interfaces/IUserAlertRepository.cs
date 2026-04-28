using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserAlertRepository : IRepository<UserAlert>
    {
        Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId);
    }
}