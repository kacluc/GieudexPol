using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserAlertRepository : IRepository<UserAlert>
    {
        Task<IEnumerable<UserAlert>> GetAllActiveUserAlertsAsync();
        Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId);
    }
}
