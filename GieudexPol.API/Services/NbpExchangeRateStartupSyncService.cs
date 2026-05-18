using GieudexPol.Application.Interfaces;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.API.Services
{
    public class NbpExchangeRateStartupSyncService : IHostedService
    {
        private const string NbpSourceCode = "NBP";
        private static readonly DateTime DefaultStartDate = new(2026, 1, 1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<NbpExchangeRateStartupSyncService> _logger;

        public NbpExchangeRateStartupSyncService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<NbpExchangeRateStartupSyncService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (!await WaitForDatabaseAsync(context, cancellationToken))
                {
                    _logger.LogWarning("NBP startup sync skipped because the database is not available.");
                    return;
                }

                await context.Database.MigrateAsync(cancellationToken);

                if (_environment.IsDevelopment())
                {
                    await DevelopmentDataSeeder.SeedAsync(scope.ServiceProvider);
                }

                var today = DateTime.Today;
                var syncFrom = await GetNextMissingDateAsync(context, cancellationToken);

                if (syncFrom > today)
                {
                    _logger.LogInformation("NBP startup sync skipped. Local NBP rates are already up to date.");
                    return;
                }

                var syncService = scope.ServiceProvider.GetRequiredService<IExchangeRateSyncService>();
                var result = await syncService.SyncNbpRatesAsync(syncFrom, today, cancellationToken);

                _logger.LogInformation(
                    "NBP startup sync completed. Range {From:yyyy-MM-dd} - {To:yyyy-MM-dd}, added {Added}, skipped {Skipped}, tables {Tables}.",
                    result.From,
                    result.To,
                    result.Added,
                    result.Skipped,
                    result.TablesFetched);

                foreach (var warning in result.Warnings)
                {
                    _logger.LogWarning("NBP startup sync warning: {Warning}", warning);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NBP startup sync failed. The API will continue to start.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task<DateTime> GetNextMissingDateAsync(
            ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            var configuredStartDate = GetConfiguredStartDate();

            var latestNbpRateDate = await context.ExchangeRates
                .AsNoTracking()
                .Where(rate => rate.RateSource.Code == NbpSourceCode)
                .MaxAsync(rate => (DateTime?)rate.EffectiveDate, cancellationToken);

            if (!latestNbpRateDate.HasValue)
            {
                return configuredStartDate;
            }

            var nextDate = latestNbpRateDate.Value.Date.AddDays(1);
            return nextDate < configuredStartDate ? configuredStartDate : nextDate;
        }

        private DateTime GetConfiguredStartDate()
        {
            var configuredValue = _configuration["NbpSync:StartDate"];
            return DateTime.TryParse(configuredValue, out var configuredDate)
                ? configuredDate.Date
                : DefaultStartDate;
        }

        private async Task<bool> WaitForDatabaseAsync(
            ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            const int maxAttempts = 10;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (await context.Database.CanConnectAsync(cancellationToken))
                {
                    return true;
                }

                _logger.LogWarning(
                    "Database is not available for NBP startup sync. Attempt {Attempt}/{MaxAttempts}.",
                    attempt,
                    maxAttempts);

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }

            return false;
        }
    }
}
