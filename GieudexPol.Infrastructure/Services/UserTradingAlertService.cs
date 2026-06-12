using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Services
{
    public class UserTradingAlertService : IUserTradingAlertService
    {
        private readonly ApplicationDbContext _context;

        public UserTradingAlertService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<UserTradingAlert>> GetUserAlertsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserTradingAlerts
                .AsNoTracking()
                .Include(alert => alert.TradingPair)
                    .ThenInclude(pair => pair.BaseCurrency)
                .Include(alert => alert.TradingPair)
                    .ThenInclude(pair => pair.QuoteCurrency)
                .Where(alert => alert.UserId == userId)
                .OrderByDescending(alert => alert.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserTradingAlert?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserTradingAlerts
                .Include(alert => alert.TradingPair)
                    .ThenInclude(pair => pair.BaseCurrency)
                .Include(alert => alert.TradingPair)
                    .ThenInclude(pair => pair.QuoteCurrency)
                .SingleOrDefaultAsync(alert => alert.Id == id, cancellationToken);
        }

        public async Task CreateAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default)
        {
            await ValidateAsync(alert, cancellationToken);
            alert.CreatedDate = DateTime.UtcNow;
            alert.IsActive = true;
            alert.IsAcknowledged = false;
            alert.AcknowledgedDate = null;
            _context.UserTradingAlerts.Add(alert);
            await _context.SaveChangesAsync(cancellationToken);
            await LoadPairAsync(alert, cancellationToken);
        }

        public async Task UpdateAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default)
        {
            if (alert.TriggeredDate.HasValue)
            {
                throw new ArgumentException("Spelnionego alertu nie mozna edytowac.");
            }

            await ValidateAsync(alert, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await LoadPairAsync(alert, cancellationToken);
        }

        public async Task DeleteAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default)
        {
            _context.UserTradingAlerts.Remove(alert);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> AcknowledgeAsync(
            int alertId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var alert = await _context.UserTradingAlerts.SingleOrDefaultAsync(
                item => item.Id == alertId && item.UserId == userId,
                cancellationToken);
            if (alert == null || !alert.TriggeredDate.HasValue)
            {
                return false;
            }

            if (!alert.IsAcknowledged)
            {
                alert.IsAcknowledged = true;
                alert.AcknowledgedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }

        private async Task ValidateAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(alert.EventType))
            {
                throw new ArgumentException("Nieprawidlowe zdarzenie alertu handlowego.");
            }

            if (!Enum.IsDefined(alert.Direction))
            {
                throw new ArgumentException("Nieprawidlowy kierunek ceny alertu.");
            }

            if (alert.TargetPrice <= 0)
            {
                throw new ArgumentException("Cena docelowa musi byc dodatnia.");
            }

            if (alert.MinimumAmount.HasValue && alert.MinimumAmount <= 0)
            {
                throw new ArgumentException("Minimalna ilosc musi byc dodatnia.");
            }

            var pairExists = await _context.TradingPairs.AnyAsync(
                pair => pair.Id == alert.TradingPairId && pair.IsActive,
                cancellationToken);
            if (!pairExists)
            {
                throw new ArgumentException("Wybrana para walutowa nie istnieje lub jest nieaktywna.");
            }
        }

        private async Task LoadPairAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken)
        {
            await _context.Entry(alert)
                .Reference(item => item.TradingPair)
                .Query()
                .Include(pair => pair.BaseCurrency)
                .Include(pair => pair.QuoteCurrency)
                .LoadAsync(cancellationToken);
        }
    }
}
