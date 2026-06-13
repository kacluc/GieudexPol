using GieudexPol.Application.Interfaces;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.API.Services
{
    public class ExchangeRateStartupSyncService : BackgroundService
    {
        private static readonly string[] SourceCodes =
        [
            "NBP",
            "ECB",
            "RIKSBANK",
            "BOE",
            "BOC",
            "CNB",
            "NORGES",
            "BNR"
        ];
        private static readonly HashSet<string> SyntheticRateSourceCodes =
            new(SourceCodes.Where(code => code != "NBP"), StringComparer.OrdinalIgnoreCase);

        private static readonly DateTime DefaultStartDate = new(2026, 1, 1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ExchangeRateStartupSyncService> _logger;

        public ExchangeRateStartupSyncService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<ExchangeRateStartupSyncService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (!await WaitForDatabaseServerAsync(context, cancellationToken))
                {
                    _logger.LogWarning("Startup exchange-rate sync skipped because the database is not available.");
                    return;
                }

                await context.Database.MigrateAsync(cancellationToken);

                if (_environment.IsDevelopment())
                {
                    await DevelopmentDataSeeder.SeedAsync(scope.ServiceProvider);
                }

                var today = DateTime.Today;
                var expectedPublicationDate = ResolveExpectedPublicationDate(today);
                var syncService = scope.ServiceProvider.GetRequiredService<IExchangeRateSyncService>();

                foreach (var sourceCode in SourceCodes)
                {
                    await SynchronizeSourceIfRequiredAsync(
                        context,
                        syncService,
                        sourceCode,
                        expectedPublicationDate,
                        today,
                        cancellationToken);
                }

                await DevelopmentDataSeeder.SeedSystemAccountsAsync(
                    scope.ServiceProvider,
                    initializeLiquidity: _environment.IsDevelopment());
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Startup exchange-rate sync was canceled because the host is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Startup exchange-rate sync failed. The API will continue to start.");
            }
        }

        private async Task SynchronizeSourceIfRequiredAsync(
            ApplicationDbContext context,
            IExchangeRateSyncService syncService,
            string sourceCode,
            DateTime expectedPublicationDate,
            DateTime today,
            CancellationToken cancellationToken)
        {
            try
            {
                var latestRateDate = await context.ExchangeRates
                    .AsNoTracking()
                    .Where(rate => rate.RateSource.Code == sourceCode)
                    .MaxAsync(rate => (DateTime?)rate.EffectiveDate, cancellationToken);

                var legacySyntheticRateDate = SyntheticRateSourceCodes.Contains(sourceCode)
                    ? await context.ExchangeRates
                        .AsNoTracking()
                        .Where(rate =>
                            rate.RateSource.Code == sourceCode &&
                            rate.BuyPrice == rate.SellPrice)
                        .MinAsync(rate => (DateTime?)rate.EffectiveDate, cancellationToken)
                    : null;

                if (!legacySyntheticRateDate.HasValue &&
                    latestRateDate.HasValue &&
                    latestRateDate.Value.Date >= expectedPublicationDate)
                {
                    _logger.LogInformation(
                        "{SourceCode} startup sync skipped. Local rates include publication date {Date:yyyy-MM-dd}.",
                        sourceCode,
                        latestRateDate.Value.Date);
                    return;
                }

                var syncFrom = legacySyntheticRateDate?.Date ??
                    (latestRateDate.HasValue
                        ? latestRateDate.Value.Date.AddDays(1)
                        : GetConfiguredStartDate());

                var result = await syncService.SyncRatesAsync(
                    sourceCode,
                    syncFrom,
                    today,
                    cancellationToken);

                _logger.LogInformation(
                    "{SourceCode} startup sync completed. Range {From:yyyy-MM-dd} - {To:yyyy-MM-dd}, added {Added}, skipped {Skipped}, tables {Tables}.",
                    sourceCode,
                    result.From,
                    result.To,
                    result.Added,
                    result.Skipped,
                    result.TablesFetched);

                foreach (var warning in result.Warnings)
                {
                    _logger.LogWarning("{SourceCode} startup sync warning: {Warning}", sourceCode, warning);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{SourceCode} startup sync failed. Other sources will still be checked.", sourceCode);
            }
        }

        private DateTime GetConfiguredStartDate()
        {
            var configuredValue = _configuration["NbpSync:StartDate"];
            return DateTime.TryParse(configuredValue, out var configuredDate)
                ? configuredDate.Date
                : DefaultStartDate;
        }

        private static DateTime ResolveExpectedPublicationDate(DateTime date)
        {
            var expectedDate = date.Date;

            while (expectedDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                expectedDate = expectedDate.AddDays(-1);
            }

            return expectedDate;
        }

        private async Task<bool> WaitForDatabaseServerAsync(
            ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            const int maxAttempts = 10;
            var connectionStringBuilder = new SqlConnectionStringBuilder(
                context.Database.GetDbConnection().ConnectionString)
            {
                InitialCatalog = "master",
                ConnectTimeout = 3
            };

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
                    await connection.OpenAsync(cancellationToken);
                    return true;
                }
                catch (SqlException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "SQL Server is not available for startup exchange-rate sync. Attempt {Attempt}/{MaxAttempts}.",
                        attempt,
                        maxAttempts);
                }

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }

            return false;
        }
    }
}
