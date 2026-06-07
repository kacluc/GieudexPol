using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class UserAlertRepository : GenericRepository<UserAlert>, IUserAlertRepository
    {
        public UserAlertRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserAlert>> GetAllActiveUserAlertsAsync()
        {
            return await _context.UserAlerts
                                 .Where(a => a.IsActive)
                                 .Include(a => a.Currency)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId)
        {
            return await _context.UserAlerts
                                 .Where(a => a.UserId == userId)
                                 .Include(a => a.Currency)
                                 .ToListAsync();
        }
    }
}
