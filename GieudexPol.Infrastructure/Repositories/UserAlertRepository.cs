using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class UserAlertRepository : GenericRepository<UserAlert>, IUserAlertRepository
    {
        public async Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId)
        {
            return await Task.FromResult(_data.Where(ua => ua.User.Id == userId).ToList());
        }
    }
}