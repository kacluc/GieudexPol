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
                                 .Where(a => a.Status == AlertStatus.Active)
                                 .Include(a => a.Currency)
                                 .Include(a => a.RateSource)
                                 .Include(a => a.Logs)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId)
        {
            return await _context.UserAlerts
                                 .Where(a => a.UserId == userId)
                                 .Include(a => a.Currency)
                                 .Include(a => a.RateSource)
                                 .Include(a => a.Logs)
                                 .OrderByDescending(a => a.CreatedDate)
                                 .ToListAsync();
        }

        public override async Task DeleteAsync(UserAlert alert)
        {
            var logs = await _context.AlertLogs
                .Where(log => log.UserAlertId == alert.Id)
                .ToListAsync();
            _context.AlertLogs.RemoveRange(logs);
            _context.UserAlerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }
}
