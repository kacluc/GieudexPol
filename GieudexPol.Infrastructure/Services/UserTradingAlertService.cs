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
                .Include(alert => alert.Logs)
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
                .Include(alert => alert.Logs)
                .SingleOrDefaultAsync(alert => alert.Id == id, cancellationToken);
        }

        public async Task CreateAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default)
        {
            await ValidateAsync(alert, cancellationToken);
            alert.CreatedDate = DateTime.UtcNow;
            alert.Status = AlertStatus.Active;
            _context.UserTradingAlerts.Add(alert);
            await _context.SaveChangesAsync(cancellationToken);
            await LoadPairAsync(alert, cancellationToken);
        }

        public async Task UpdateAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default)
        {
            await ValidateAsync(alert, cancellationToken);
            if (alert.Status != AlertStatus.Fulfilled)
            {
                alert.TriggeredDate = null;
            }
            await _context.SaveChangesAsync(cancellationToken);
            await LoadPairAsync(alert, cancellationToken);
        }

        public async Task DeleteAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default)
        {
            var logs = await _context.AlertLogs
                .Where(log => log.UserTradingAlertId == alert.Id)
                .ToListAsync(cancellationToken);
            _context.AlertLogs.RemoveRange(logs);
            _context.UserTradingAlerts.Remove(alert);
            await _context.SaveChangesAsync(cancellationToken);
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

            if (!Enum.IsDefined(alert.Status))
            {
                throw new ArgumentException("Nieprawidlowy status alertu.");
            }

            if (alert.EventType == TradingAlertEvent.SellOrder &&
                alert.Direction != ThresholdDirection.BelowOrEqual)
            {
                throw new ArgumentException(
                    "Alert kupna waluty monitoruje oferty sprzedazy tylko z warunkiem <=.");
            }

            if (alert.EventType == TradingAlertEvent.BuyOrder &&
                alert.Direction != ThresholdDirection.AboveOrEqual)
            {
                throw new ArgumentException(
                    "Alert sprzedazy waluty monitoruje oferty kupna tylko z warunkiem >=.");
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
