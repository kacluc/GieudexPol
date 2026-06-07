using System.Collections.Concurrent;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Application.Settings;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GieudexPol.Infrastructure.Services
{
    public class ExchangeRateSyncService : IExchangeRateSyncService
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> SyncLocks =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> SyntheticRateSourceCodes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "ECB",
                "BOE",
                "RIKSBANK",
                "CNB",
                "NORGES",
                "BNR"
            };

        private readonly ApplicationDbContext _context;
        private readonly IEnumerable<IExternalExchangeRateClient> _exchangeRateClients;
        private readonly ILogger<ExchangeRateSyncService> _logger;
        private readonly ExchangeRateSettings _settings;

        public ExchangeRateSyncService(
            ApplicationDbContext context,
            IEnumerable<IExternalExchangeRateClient> exchangeRateClients,
            ILogger<ExchangeRateSyncService> logger,
            IOptions<ExchangeRateSettings> settings)
        {
            _context = context;
            _exchangeRateClients = exchangeRateClients;
            _logger = logger;
            _settings = settings.Value;
        }

        public Task<NbpSyncResultDto> SyncCurrentYearRatesAsync(
            string sourceCode,
            CancellationToken cancellationToken = default)
        {
            var from = new DateTime(DateTime.Today.Year, 1, 1);
            var to = DateTime.Today;

            return SyncRatesAsync(sourceCode, from, to, cancellationToken);
        }

        public async Task<NbpSyncResultDto> SyncNbpRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            return await SyncRatesAsync("NBP", from, to, cancellationToken);
        }

        public async Task<NbpSyncResultDto> SyncRatesAsync(
            string sourceCode,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            from = from.Date;
            to = to.Date;
            sourceCode = sourceCode.Trim().ToUpperInvariant();

            if (from > to)
            {
                throw new ArgumentException("From date cannot be later than to date.");
            }

            var today = DateTime.Today;
            if (to > today)
            {
                throw new ArgumentException("To date cannot be later than today.");
            }

            var exchangeRateClient = _exchangeRateClients.SingleOrDefault(client =>
                string.Equals(client.SourceCode, sourceCode, StringComparison.OrdinalIgnoreCase));

            if (exchangeRateClient == null)
            {
                throw new InvalidOperationException($"Exchange rate source '{sourceCode}' is not supported.");
            }

            var syncLock = SyncLocks.GetOrAdd(exchangeRateClient.SourceCode, _ => new SemaphoreSlim(1, 1));
            await syncLock.WaitAsync(cancellationToken);

            try
            {
                try
                {
                    return await SyncRatesCoreAsync(exchangeRateClient, from, to, cancellationToken);
                }
                catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                {
                    _logger.LogWarning(
                        ex,
                        "A concurrent {SourceCode} sync saved matching records first. Retrying after reloading persisted data.",
                        exchangeRateClient.SourceCode);

                    _context.ChangeTracker.Clear();
                    return await SyncRatesCoreAsync(exchangeRateClient, from, to, cancellationToken);
                }
            }
            finally
            {
                syncLock.Release();
            }
        }

        private async Task<NbpSyncResultDto> SyncRatesCoreAsync(
            IExternalExchangeRateClient exchangeRateClient,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken)
        {
            var result = new NbpSyncResultDto
            {
                From = from,
                To = to
            };

            _logger.LogInformation(
                "Starting {SourceCode} exchange rate sync from {From} to {To}.",
                exchangeRateClient.SourceCode,
                from,
                to);

            var rateSource = await GetOrCreateRateSourceAsync(exchangeRateClient, cancellationToken);
            var currencies = await _context.Currencies.ToDictionaryAsync(
                currency => currency.Symbol,
                cancellationToken);
            if (!currencies.ContainsKey("PLN"))
            {
                var baseCurrency = new Currency
                {
                    Symbol = "PLN",
                    Name = "Polish Zloty",
                    IsActive = true
                };

                currencies[baseCurrency.Symbol] = baseCurrency;
                await _context.Currencies.AddAsync(baseCurrency, cancellationToken);
            }

            var existingRates = await LoadExistingRatesAsync(rateSource.Id, from, to, cancellationToken);

            foreach (var (rangeFrom, rangeTo) in SplitIntoRanges(from, to, exchangeRateClient.MaxRangeDays))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rangeLabel = $"{rangeFrom:yyyy-MM-dd} - {rangeTo:yyyy-MM-dd}";
                result.ProcessedRanges.Add(rangeLabel);
                _logger.LogInformation("Fetching {SourceCode} exchange rates range {Range}.", exchangeRateClient.SourceCode, rangeLabel);

                IReadOnlyList<ExternalExchangeRateTableDto> tables;
                try
                {
                    tables = await exchangeRateClient.GetBuySellRatesAsync(rangeFrom, rangeTo, cancellationToken);
                }
                catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException)
                {
                    result.Warnings.Add($"Range {rangeLabel}: {ex.Message}");
                    _logger.LogWarning(ex, "{SourceCode} range {Range} failed.", exchangeRateClient.SourceCode, rangeLabel);
                    continue;
                }

                result.TablesFetched += tables.Count;

                if (tables.Count == 0)
                {
                    result.Warnings.Add($"Range {rangeLabel}: {exchangeRateClient.SourceCode} returned no exchange rate data.");
                    continue;
                }

                foreach (var table in tables)
                {
                    var effectiveDate = table.EffectiveDate.Date;

                    foreach (var rate in table.Rates)
                    {
                        var currencyCode = rate.CurrencyCode.Trim().ToUpperInvariant();
                        if (!TradingCurrencyCatalog.Contains(currencyCode))
                        {
                            continue;
                        }

                        if (!currencies.TryGetValue(currencyCode, out var currency))
                        {
                            currency = new Currency
                            {
                                Symbol = currencyCode,
                                Name = rate.CurrencyName,
                                IsActive = true
                            };

                            currencies[currencyCode] = currency;
                            await _context.Currencies.AddAsync(currency, cancellationToken);
                        }

                        var existingKey = new ExistingRateKey(currencyCode, effectiveDate);
                        if (existingRates.TryGetValue(existingKey, out var existingRate))
                        {
                            if (IsSyntheticRateSource(exchangeRateClient.SourceCode))
                            {
                                var (updatedBuyPrice, updatedSellPrice, updatedMidPrice) = GetStoredPrices(
                                    exchangeRateClient.SourceCode,
                                    rate);
                                existingRate.BuyPrice = updatedBuyPrice;
                                existingRate.SellPrice = updatedSellPrice;
                                existingRate.MidPrice = updatedMidPrice;
                                existingRate.FetchedAt = DateTime.UtcNow;
                            }

                            result.Skipped++;
                            continue;
                        }

                        var (buyPrice, sellPrice, midPrice) = GetStoredPrices(
                            exchangeRateClient.SourceCode,
                            rate);

                        var exchangeRate = new ExchangeRate
                        {
                            Currency = currency,
                            RateSource = rateSource,
                            BuyPrice = buyPrice,
                            SellPrice = sellPrice,
                            MidPrice = midPrice,
                            EffectiveDate = effectiveDate,
                            FetchedAt = DateTime.UtcNow
                        };
                        await _context.ExchangeRates.AddAsync(exchangeRate, cancellationToken);

                        existingRates.Add(existingKey, exchangeRate);
                        result.Added++;
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Finished {SourceCode} exchange rate sync. Added {Added}, skipped {Skipped}, tables fetched {TablesFetched}.",
                exchangeRateClient.SourceCode,
                result.Added,
                result.Skipped,
                result.TablesFetched);

            return result;
        }

        private (decimal BuyPrice, decimal SellPrice, decimal MidPrice) GetStoredPrices(
            string sourceCode,
            ExternalExchangeRateItemDto rate)
        {
            if (IsSyntheticRateSource(sourceCode))
            {
                var referenceRate = rate.ReferenceRate ??
                    decimal.Round((rate.BuyPrice + rate.SellPrice) / 2m, 6, MidpointRounding.AwayFromZero);

                // ECB, BOE, RIKSBANK, CNB, NORGES and BNR publish reference/mid rates,
                // not official bid/ask tables. BuyPrice and SellPrice are synthetic values
                // calculated from the reference rate using configured spread.
                var (buyPrice, sellPrice) =
                    ExchangeRateSpreadCalculator.CalculateSyntheticBidAsk(
                        referenceRate,
                        _settings.SyntheticSpreadPercent);

                return (
                    buyPrice,
                    sellPrice,
                    decimal.Round(referenceRate, 4, MidpointRounding.AwayFromZero));
            }

            return (
                rate.BuyPrice,
                rate.SellPrice,
                decimal.Round(
                    (rate.BuyPrice + rate.SellPrice) / 2m,
                    4,
                    MidpointRounding.AwayFromZero));
        }

        private static bool IsSyntheticRateSource(string sourceCode)
        {
            return SyntheticRateSourceCodes.Contains(sourceCode);
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException { Number: 2601 or 2627 };
        }

        private async Task<RateSource> GetOrCreateRateSourceAsync(
            IExternalExchangeRateClient exchangeRateClient,
            CancellationToken cancellationToken)
        {
            var rateSource = await _context.RateSources
                .FirstOrDefaultAsync(source => source.Code == exchangeRateClient.SourceCode, cancellationToken);

            if (rateSource != null)
            {
                rateSource.Name = exchangeRateClient.SourceName;
                rateSource.IsActive = true;
                return rateSource;
            }

            rateSource = new RateSource
            {
                Code = exchangeRateClient.SourceCode,
                Name = exchangeRateClient.SourceName,
                IsActive = true
            };

            await _context.RateSources.AddAsync(rateSource, cancellationToken);
            return rateSource;
        }

        private async Task<Dictionary<ExistingRateKey, ExchangeRate>> LoadExistingRatesAsync(
            int rateSourceId,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken)
        {
            if (rateSourceId == 0)
            {
                return new Dictionary<ExistingRateKey, ExchangeRate>();
            }

            var rates = await _context.ExchangeRates
                .Include(rate => rate.Currency)
                .Where(rate =>
                    rate.RateSourceId == rateSourceId &&
                    rate.EffectiveDate >= from &&
                    rate.EffectiveDate <= to)
                .ToListAsync(cancellationToken);

            return rates.ToDictionary(
                rate => new ExistingRateKey(rate.Currency.Symbol, rate.EffectiveDate.Date));
        }

        private static IEnumerable<(DateTime From, DateTime To)> SplitIntoRanges(DateTime from, DateTime to, int maxRangeDays)
        {
            if (maxRangeDays <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRangeDays), "Max range days must be greater than zero.");
            }

            var rangeFrom = from.Date;

            while (rangeFrom <= to.Date)
            {
                var rangeTo = rangeFrom.AddDays(maxRangeDays - 1);
                if (rangeTo > to.Date)
                {
                    rangeTo = to.Date;
                }

                yield return (rangeFrom, rangeTo);
                rangeFrom = rangeTo.AddDays(1);
            }
        }

        private sealed record ExistingRateKey(string CurrencyCode, DateTime EffectiveDate);
    }
}
