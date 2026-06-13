using System.Data;
using System.Globalization;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Services
{
    public class TradingAlertEvaluationService : ITradingAlertEvaluationService
    {
        private readonly ApplicationDbContext _context;

        public TradingAlertEvaluationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<AlertEvaluationResult> EvaluateAllActiveAlertsAsync(
            CancellationToken cancellationToken = default)
        {
            return EvaluateAsync(null, null, cancellationToken);
        }

        public Task<AlertEvaluationResult> EvaluateAlertAsync(
            int alertId,
            CancellationToken cancellationToken = default)
        {
            return EvaluateAsync(alertId, null, cancellationToken);
        }

        public Task<AlertEvaluationResult> EvaluatePairAsync(
            int tradingPairId,
            CancellationToken cancellationToken = default)
        {
            return EvaluateAsync(null, tradingPairId, cancellationToken);
        }

        private async Task<AlertEvaluationResult> EvaluateAsync(
            int? alertId,
            int? tradingPairId,
            CancellationToken cancellationToken)
        {
            if (!_context.Database.IsRelational())
            {
                return await EvaluateCoreAsync(alertId, tradingPairId, cancellationToken);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var result = await EvaluateCoreAsync(
                    alertId,
                    tradingPairId,
                    cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            });
        }

        private async Task<AlertEvaluationResult> EvaluateCoreAsync(
            int? alertId,
            int? tradingPairId,
            CancellationToken cancellationToken)
        {
            var query = _context.UserTradingAlerts
                .Include(alert => alert.TradingPair)
                    .ThenInclude(pair => pair.BaseCurrency)
                .Include(alert => alert.TradingPair)
                    .ThenInclude(pair => pair.QuoteCurrency)
                .Include(alert => alert.Logs)
                .Where(alert =>
                    alert.Status == AlertStatus.Active ||
                    alert.Status == AlertStatus.Fulfilled);

            if (alertId.HasValue)
            {
                query = query.Where(alert => alert.Id == alertId.Value);
            }

            if (tradingPairId.HasValue)
            {
                query = query.Where(alert => alert.TradingPairId == tradingPairId.Value);
            }

            var alerts = await query
                .OrderBy(alert => alert.Id)
                .ToListAsync(cancellationToken);
            var result = new AlertEvaluationResult();

            foreach (var alert in alerts)
            {
                result.EvaluatedAlertsCount++;
                var trigger = alert.EventType == TradingAlertEvent.TradeExecution
                    ? await FindExecutionTriggerAsync(alert, cancellationToken)
                    : await FindOrderTriggerAsync(alert, cancellationToken);

                if (trigger == null)
                {
                    alert.Status = AlertStatus.Active;
                    continue;
                }

                alert.Status = AlertStatus.Fulfilled;
                var hasLoggedTrigger = alert.Logs.Any(
                    log => log.SourceSummary == trigger.EventKey);
                if (hasLoggedTrigger)
                {
                    continue;
                }

                alert.TriggeredDate = DateTime.UtcNow;
                var message = BuildNotificationMessage(alert, trigger);
                _context.AlertLogs.Add(new AlertLog
                {
                    UserTradingAlert = alert,
                    Message = message,
                    CreatedDate = alert.TriggeredDate.Value,
                    CurrentPrice = trigger.Price,
                    CurrentAmount = trigger.Amount,
                    SourceSummary = trigger.EventKey,
                    EffectiveDate = trigger.OccurredAt
                });
                _context.Notifications.Add(new Notification
                {
                    UserId = alert.UserId,
                    Message = message,
                    CreatedDate = DateTime.UtcNow,
                    IsRead = false
                });

                result.TriggeredAlertsCount++;
                result.NotificationsCreatedCount++;
                result.Details.Add(message);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return result;
        }

        private async Task<TradingTrigger?> FindOrderTriggerAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken)
        {
            var side = alert.EventType == TradingAlertEvent.BuyOrder
                ? OrderSide.Buy
                : OrderSide.Sell;
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(order =>
                    order.TradingPairId == alert.TradingPairId &&
                    order.UserId != alert.UserId &&
                    order.Side == side &&
                    (order.Status == OrderStatus.Open ||
                     order.Status == OrderStatus.PartiallyFilled) &&
                    order.RemainingAmount > 0)
                .Select(order => new
                {
                    order.Id,
                    order.Price,
                    order.RemainingAmount,
                    order.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var matchingLevel = orders
                .GroupBy(order => order.Price)
                .Select(level => new
                {
                    Price = level.Key,
                    Amount = level.Sum(order => order.RemainingAmount),
                    OccurredAt = level.Max(order => order.CreatedAt),
                    EventKey = $"OrderLevel:{side}:{level.Key.ToString(CultureInfo.InvariantCulture)}:" +
                               string.Join(",", level.Select(order => order.Id).OrderBy(id => id))
                })
                .OrderBy(level => side == OrderSide.Buy ? -level.Price : level.Price)
                .FirstOrDefault();

            if (matchingLevel == null ||
                !MeetsPrice(alert, matchingLevel.Price) ||
                !MeetsAmount(alert, matchingLevel.Amount))
            {
                return null;
            }

            return new TradingTrigger(
                matchingLevel.Price,
                matchingLevel.Amount,
                matchingLevel.OccurredAt,
                matchingLevel.EventKey);
        }

        private async Task<TradingTrigger?> FindExecutionTriggerAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken)
        {
            var executions = await _context.TradeExecutions
                .AsNoTracking()
                .Where(execution =>
                    execution.TradingPairId == alert.TradingPairId &&
                    execution.ExecutedAt >= alert.CreatedDate)
                .OrderByDescending(execution => execution.ExecutedAt)
                .ThenByDescending(execution => execution.Id)
                .Select(execution => new
                {
                    execution.Id,
                    execution.Price,
                    execution.Amount,
                    execution.ExecutedAt
                })
                .ToListAsync(cancellationToken);

            var execution = executions.FirstOrDefault();

            return execution == null ||
                   !MeetsPrice(alert, execution.Price) ||
                   !MeetsAmount(alert, execution.Amount)
                ? null
                : new TradingTrigger(
                    execution.Price,
                    execution.Amount,
                    execution.ExecutedAt,
                    $"TradeExecution:{execution.Id}");
        }

        private static bool MeetsPrice(UserTradingAlert alert, decimal price)
        {
            var direction = GetEffectiveDirection(alert);
            return direction == ThresholdDirection.AboveOrEqual
                ? price >= alert.TargetPrice
                : price <= alert.TargetPrice;
        }

        private static ThresholdDirection GetEffectiveDirection(UserTradingAlert alert)
        {
            return alert.EventType switch
            {
                TradingAlertEvent.SellOrder => ThresholdDirection.BelowOrEqual,
                TradingAlertEvent.BuyOrder => ThresholdDirection.AboveOrEqual,
                _ => alert.Direction
            };
        }

        private static bool MeetsAmount(UserTradingAlert alert, decimal amount)
        {
            return !alert.MinimumAmount.HasValue || amount >= alert.MinimumAmount.Value;
        }

        private static string BuildNotificationMessage(
            UserTradingAlert alert,
            TradingTrigger trigger)
        {
            var pair = alert.TradingPair.BaseCurrency.Symbol + "/" +
                       alert.TradingPair.QuoteCurrency.Symbol;
            var eventLabel = alert.EventType switch
            {
                TradingAlertEvent.BuyOrder => "chcesz sprzedac - najlepsza oferta kupna",
                TradingAlertEvent.SellOrder => "chcesz kupic - najtansza oferta sprzedazy",
                TradingAlertEvent.TradeExecution => "wykonana transakcja",
                _ => "zdarzenie na rynku"
            };
            var direction = GetEffectiveDirection(alert) == ThresholdDirection.AboveOrEqual
                ? ">="
                : "<=";
            var price = trigger.Price.ToString("0.####", CultureInfo.InvariantCulture);
            var amount = trigger.Amount.ToString("0.####", CultureInfo.InvariantCulture);

            return $"Alert rynku {pair}: {eventLabel} spelnila warunek " +
                   $"{direction} {alert.TargetPrice.ToString("0.####", CultureInfo.InvariantCulture)}. " +
                   $"Cena: {price} {alert.TradingPair.QuoteCurrency.Symbol}, " +
                   $"ilosc: {amount} {alert.TradingPair.BaseCurrency.Symbol}, " +
                   $"czas: {trigger.OccurredAt:yyyy-MM-dd HH:mm} UTC.";
        }

        private sealed record TradingTrigger(
            decimal Price,
            decimal Amount,
            DateTime OccurredAt,
            string EventKey);
    }
}
