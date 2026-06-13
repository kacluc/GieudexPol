using System.Globalization;
using System.Data;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Services
{
    public class AlertEvaluationService : IAlertEvaluationService
    {
        private readonly ApplicationDbContext _context;

        public AlertEvaluationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<AlertEvaluationResult> EvaluateAllActiveAlertsAsync(
            CancellationToken cancellationToken = default)
        {
            return EvaluateAsync(null, null, null, cancellationToken);
        }

        public Task<AlertEvaluationResult> EvaluateAlertAsync(
            int alertId,
            CancellationToken cancellationToken = default)
        {
            return EvaluateAsync(alertId, null, null, cancellationToken);
        }

        public Task<AlertEvaluationResult> EvaluateAlertsForRateAsync(
            string? currencyCode,
            string? rateSourceCode,
            CancellationToken cancellationToken = default)
        {
            return EvaluateAsync(null, currencyCode, rateSourceCode, cancellationToken);
        }

        private async Task<AlertEvaluationResult> EvaluateAsync(
            int? alertId,
            string? currencyCode,
            string? rateSourceCode,
            CancellationToken cancellationToken)
        {
            if (!_context.Database.IsRelational())
            {
                return await EvaluateCoreAsync(
                    alertId,
                    currencyCode,
                    rateSourceCode,
                    cancellationToken);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var result = await EvaluateCoreAsync(
                    alertId,
                    currencyCode,
                    rateSourceCode,
                    cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            });
        }

        private async Task<AlertEvaluationResult> EvaluateCoreAsync(
            int? alertId,
            string? currencyCode,
            string? rateSourceCode,
            CancellationToken cancellationToken)
        {
            var result = new AlertEvaluationResult();
            var normalizedCurrencyCode = NormalizeCode(currencyCode);
            var normalizedSourceCode = NormalizeCode(rateSourceCode);

            int? requestedSourceId = null;
            if (normalizedSourceCode != null)
            {
                var requestedSource = await _context.RateSources
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        source => source.Code == normalizedSourceCode && source.IsActive,
                        cancellationToken);
                if (requestedSource == null)
                {
                    throw new ArgumentException(
                        $"Aktywne zrodlo kursu {normalizedSourceCode} nie istnieje.");
                }

                requestedSourceId = requestedSource.Id;
            }

            var alertsQuery = _context.UserAlerts
                .Include(alert => alert.Currency)
                .Include(alert => alert.RateSource)
                .Include(alert => alert.EvaluationStates)
                .Where(alert =>
                    alert.Status == AlertStatus.Active ||
                    alert.Status == AlertStatus.Fulfilled);

            if (alertId.HasValue)
            {
                alertsQuery = alertsQuery.Where(alert => alert.Id == alertId.Value);
            }

            if (normalizedCurrencyCode != null)
            {
                alertsQuery = alertsQuery.Where(
                    alert => alert.Currency.Symbol == normalizedCurrencyCode);
            }

            if (requestedSourceId.HasValue)
            {
                alertsQuery = alertsQuery.Where(
                    alert => !alert.RateSourceId.HasValue ||
                             alert.RateSourceId == requestedSourceId.Value);
            }

            var alerts = await alertsQuery
                .OrderBy(alert => alert.Id)
                .ToListAsync(cancellationToken);

            foreach (var alert in alerts)
            {
                result.EvaluatedAlertsCount++;
                await EvaluateSingleAlertAsync(
                    alert,
                    requestedSourceId,
                    result,
                    cancellationToken);
            }

            return result;
        }

        private async Task EvaluateSingleAlertAsync(
            UserAlert alert,
            int? requestedSourceId,
            AlertEvaluationResult result,
            CancellationToken cancellationToken)
        {
            var ratesQuery = _context.ExchangeRates
                .Include(rate => rate.RateSource)
                .Where(rate =>
                    rate.CurrencyId == alert.CurrencyId &&
                    rate.RateSource.IsActive);

            if (requestedSourceId.HasValue)
            {
                ratesQuery = ratesQuery.Where(
                    rate => rate.RateSourceId == requestedSourceId.Value);
            }
            else if (alert.RateSourceId.HasValue)
            {
                ratesQuery = ratesQuery.Where(
                    rate => rate.RateSourceId == alert.RateSourceId.Value);
            }

            var rates = await ratesQuery
                .OrderByDescending(rate => rate.EffectiveDate)
                .ThenByDescending(rate => rate.FetchedAt)
                .ToListAsync(cancellationToken);

            var triggerDetails = new List<TriggerDetail>();
            var isCurrentlyFulfilled = false;
            foreach (var sourceRates in rates.GroupBy(rate => rate.RateSourceId))
            {
                var currentRate = sourceRates.First();
                var state = alert.EvaluationStates.SingleOrDefault(
                    item => item.RateSourceId == currentRate.RateSourceId);
                var isNewEffectiveDate = state == null ||
                    state.LastEvaluatedEffectiveDate < currentRate.EffectiveDate;

                var currentPrice = SelectPrice(currentRate, alert.PriceSide);
                decimal? changePercent = null;
                var isTriggered = false;

                if (alert.AlertType == AlertType.Threshold)
                {
                    isTriggered = EvaluateThreshold(alert, currentPrice);
                }
                else
                {
                    var previousRate = SelectPreviousRate(
                        sourceRates.Skip(1),
                        currentRate,
                        alert.TimeFrameHours);

                    if (previousRate != null)
                    {
                        var previousPrice = SelectPrice(previousRate, alert.PriceSide);
                        if (previousPrice > 0)
                        {
                            changePercent =
                                (currentPrice - previousPrice) / previousPrice * 100m;
                            isTriggered = alert.AlertType == AlertType.PriceIncrease
                                ? changePercent >= alert.PercentageChange
                                : changePercent <= -alert.PercentageChange;
                        }
                    }
                }

                isCurrentlyFulfilled |= isTriggered;

                if (isNewEffectiveDate && state == null)
                {
                    state = new UserAlertEvaluationState
                    {
                        UserAlert = alert,
                        RateSourceId = currentRate.RateSourceId,
                        LastEvaluatedEffectiveDate = currentRate.EffectiveDate
                    };
                    alert.EvaluationStates.Add(state);
                }
                else if (isNewEffectiveDate)
                {
                    state!.LastEvaluatedEffectiveDate = currentRate.EffectiveDate;
                }

                if (isNewEffectiveDate && isTriggered)
                {
                    triggerDetails.Add(new TriggerDetail(
                        currentRate.RateSource.Code,
                        currentPrice,
                        currentRate.EffectiveDate,
                        changePercent));
                }
            }

            alert.Status = isCurrentlyFulfilled
                ? AlertStatus.Fulfilled
                : AlertStatus.Active;

            if (triggerDetails.Count > 0)
            {
                alert.TriggeredDate = DateTime.UtcNow;
                var message = BuildNotificationMessage(alert, triggerDetails);
                _context.AlertLogs.Add(new AlertLog
                {
                    UserAlert = alert,
                    Message = message,
                    CreatedDate = alert.TriggeredDate.Value,
                    CurrentPrice = triggerDetails.Count == 1
                        ? triggerDetails[0].CurrentPrice
                        : null,
                    SourceSummary = string.Join(
                        ", ",
                        triggerDetails.Select(detail => detail.SourceCode)),
                    EffectiveDate = triggerDetails
                        .Select(detail => detail.EffectiveDate)
                        .Distinct()
                        .Count() == 1
                            ? triggerDetails[0].EffectiveDate
                            : null
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
        }

        private static ExchangeRate? SelectPreviousRate(
            IEnumerable<ExchangeRate> olderRates,
            ExchangeRate currentRate,
            int? timeFrameHours)
        {
            if (!timeFrameHours.HasValue || timeFrameHours.Value == 24)
            {
                return olderRates.FirstOrDefault(
                    rate => rate.EffectiveDate < currentRate.EffectiveDate);
            }

            var referenceDate = currentRate.EffectiveDate.AddHours(-timeFrameHours.Value);
            return olderRates
                .Where(rate => rate.EffectiveDate <= referenceDate)
                .OrderByDescending(rate => rate.EffectiveDate)
                .ThenByDescending(rate => rate.FetchedAt)
                .FirstOrDefault();
        }

        private static decimal SelectPrice(ExchangeRate rate, AlertPriceSide priceSide)
        {
            return priceSide switch
            {
                AlertPriceSide.UserBuysCurrency => rate.SellPrice,
                AlertPriceSide.UserSellsCurrency => rate.BuyPrice,
                AlertPriceSide.MidPrice =>
                    rate.MidPrice ?? (rate.BuyPrice + rate.SellPrice) / 2m,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(priceSide),
                    priceSide,
                    "Nieprawidlowa strona ceny alertu.")
            };
        }

        private static bool EvaluateThreshold(UserAlert alert, decimal currentPrice)
        {
            if (!alert.ThresholdValue.HasValue || !alert.ThresholdDirection.HasValue)
            {
                return false;
            }

            return alert.ThresholdDirection == Domain.Entities.ThresholdDirection.AboveOrEqual
                ? currentPrice >= alert.ThresholdValue.Value
                : currentPrice <= alert.ThresholdValue.Value;
        }

        private static string BuildNotificationMessage(
            UserAlert alert,
            IReadOnlyCollection<TriggerDetail> details)
        {
            var priceSide = alert.PriceSide switch
            {
                AlertPriceSide.UserBuysCurrency => "cena kupna waluty przez uzytkownika",
                AlertPriceSide.UserSellsCurrency => "cena sprzedazy waluty przez uzytkownika",
                AlertPriceSide.MidPrice => "kurs sredni",
                _ => "monitorowana cena"
            };

            var detailText = string.Join(
                ", ",
                details.Select(detail =>
                {
                    var price = detail.CurrentPrice.ToString("0.####", CultureInfo.InvariantCulture);
                    var date = detail.EffectiveDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                    if (detail.ChangePercent.HasValue)
                    {
                        var change = detail.ChangePercent.Value.ToString(
                            "+0.##;-0.##;0",
                            CultureInfo.InvariantCulture);
                        return $"{detail.SourceCode}: {change}% (cena {price}, data {date})";
                    }

                    return $"{detail.SourceCode}: cena {price}, data {date}";
                }));

            var condition = alert.AlertType switch
            {
                AlertType.Threshold =>
                    $"{priceSide} osiagnela prog {alert.ThresholdValue:0.####} " +
                    $"({FormatThresholdDirection(alert.ThresholdDirection)})",
                AlertType.PriceIncrease =>
                    $"{priceSide} wzrosla o co najmniej {alert.PercentageChange:0.####}%",
                AlertType.PriceDrop =>
                    $"{priceSide} spadla o co najmniej {alert.PercentageChange:0.####}%",
                _ => priceSide
            };

            return $"Alert spelniony dla {alert.Currency.Symbol}: {condition}. {detailText}.";
        }

        private static string FormatThresholdDirection(ThresholdDirection? direction)
        {
            return direction == Domain.Entities.ThresholdDirection.AboveOrEqual
                ? ">="
                : "<=";
        }

        private static string? NormalizeCode(string? code)
        {
            return string.IsNullOrWhiteSpace(code)
                ? null
                : code.Trim().ToUpperInvariant();
        }

        private sealed record TriggerDetail(
            string SourceCode,
            decimal CurrentPrice,
            DateTime EffectiveDate,
            decimal? ChangePercent);
    }
}
