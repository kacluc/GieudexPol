using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Settings;
using Microsoft.Extensions.Options;

namespace GieudexPol.API.Services
{
    public class AlertMonitoringWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlertMonitoringWorker> _logger;
        private readonly TimeSpan _interval;

        public AlertMonitoringWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<AlertMonitoringSettings> settings,
            ILogger<AlertMonitoringWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _interval = TimeSpan.FromMinutes(Math.Max(1, settings.Value.IntervalMinutes));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Rozpoczynam automatyczna ewaluacje alertow.");
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider
                        .GetRequiredService<IAlertEvaluationService>();
                    var result = await service.EvaluateAllActiveAlertsAsync(stoppingToken);
                    var tradingService = scope.ServiceProvider
                        .GetRequiredService<ITradingAlertEvaluationService>();
                    var tradingResult = await tradingService
                        .EvaluateAllActiveAlertsAsync(stoppingToken);
                    _logger.LogInformation(
                        "Ewaluacja alertow kursowych zakonczona. Oceniono: {Evaluated}, " +
                        "uruchomiono: {Triggered}, powiadomienia: {Notifications}.",
                        result.EvaluatedAlertsCount,
                        result.TriggeredAlertsCount,
                        result.NotificationsCreatedCount);
                    _logger.LogInformation(
                        "Ewaluacja alertow handlowych zakonczona. Oceniono: {Evaluated}, " +
                        "uruchomiono: {Triggered}, powiadomienia: {Notifications}.",
                        tradingResult.EvaluatedAlertsCount,
                        tradingResult.TriggeredAlertsCount,
                        tradingResult.NotificationsCreatedCount);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Blad podczas automatycznej ewaluacji alertow.");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
