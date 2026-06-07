using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.Json;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;

namespace GieudexPol.Infrastructure.ExternalServices.Cnb
{
    public class CnbExchangeRateClient : IExternalExchangeRateClient
    {
        private static readonly ISet<string> SupportedCurrencyCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AUD",
            "CAD",
            "CHF",
            "CZK",
            "DKK",
            "EUR",
            "GBP",
            "HUF",
            "JPY",
            "KRW",
            "NOK",
            "PLN",
            "RON",
            "SEK",
            "TRY",
            "USD"
        };

        private readonly HttpClient _httpClient;

        public string SourceCode => "CNB";
        public string SourceName => "Czech National Bank";
        public int MaxRangeDays => 31;

        public CnbExchangeRateClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var publishedRates = new ConcurrentBag<CnbPublishedRate>();
            var requestedDates = EnumeratePublicationCheckDates(from.Date, to.Date);

            await Parallel.ForEachAsync(
                requestedDates,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 4,
                    CancellationToken = cancellationToken
                },
                async (requestedDate, token) =>
                {
                    var endpoint = $"exrates/daily?date={requestedDate:yyyy-MM-dd}&lang=EN";
                    using var response = await _httpClient.GetAsync(endpoint, token);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return;
                    }

                    response.EnsureSuccessStatusCode();

                    await using var stream = await response.Content.ReadAsStreamAsync(token);
                    using var document = await JsonDocument.ParseAsync(stream, cancellationToken: token);
                    foreach (var publishedRate in ParseDailyRates(document.RootElement))
                    {
                        publishedRates.Add(publishedRate);
                    }
                });

            return ConvertDailyRatesToPlnRates(publishedRates, from.Date, to.Date);
        }

        private static IEnumerable<DateTime> EnumeratePublicationCheckDates(DateTime from, DateTime to)
        {
            for (var requestedDate = from.Date; requestedDate <= to.Date; requestedDate = requestedDate.AddDays(1))
            {
                if (requestedDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    continue;
                }

                yield return requestedDate;
            }
        }

        public static IReadOnlyList<CnbPublishedRate> ParseDailyRates(JsonElement root)
        {
            var publishedRates = new List<CnbPublishedRate>();
            if (!root.TryGetProperty("rates", out var rates) || rates.ValueKind != JsonValueKind.Array)
            {
                return publishedRates;
            }

            foreach (var rate in rates.EnumerateArray())
            {
                if (!TryGetString(rate, "validFor", out var validForText) ||
                    !TryGetString(rate, "currencyCode", out var currencyCode) ||
                    !TryGetPositiveInt(rate, "amount", out var amount) ||
                    !TryGetPositiveDecimal(rate, "rate", out var czkRate) ||
                    !DateTime.TryParse(validForText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var validFor))
                {
                    continue;
                }

                currencyCode = currencyCode.Trim().ToUpperInvariant();
                if (!SupportedCurrencyCodes.Contains(currencyCode))
                {
                    continue;
                }

                publishedRates.Add(new CnbPublishedRate(validFor.Date, currencyCode, amount, czkRate));
            }

            return publishedRates;
        }

        public static IReadOnlyList<ExternalExchangeRateTableDto> ConvertDailyRatesToPlnRates(
            IEnumerable<CnbPublishedRate> publishedRates,
            DateTime from,
            DateTime to)
        {
            var ratesByDate = publishedRates
                .Where(rate =>
                    rate.ValidFor.Date >= from.Date &&
                    rate.ValidFor.Date <= to.Date &&
                    rate.Amount > 0 &&
                    rate.Rate > 0 &&
                    SupportedCurrencyCodes.Contains(rate.CurrencyCode))
                .GroupBy(rate => rate.ValidFor.Date)
                .OrderBy(group => group.Key)
                .Select(group => new
                {
                    Date = group.Key,
                    Rates = group
                        .GroupBy(rate => rate.CurrencyCode, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            currencyGroup => currencyGroup.Key,
                            currencyGroup => currencyGroup.Last(),
                            StringComparer.OrdinalIgnoreCase)
                });

            var tables = new List<ExternalExchangeRateTableDto>();
            foreach (var dayRates in ratesByDate)
            {
                if (!dayRates.Rates.TryGetValue("PLN", out var plnRate))
                {
                    throw CreateMissingPlnException(dayRates.Date);
                }

                var plnToCzk = plnRate.Rate / plnRate.Amount;
                if (plnToCzk <= 0)
                {
                    throw CreateMissingPlnException(dayRates.Date);
                }

                var table = new ExternalExchangeRateTableDto
                {
                    Table = "CNB",
                    Number = $"CNB/{dayRates.Date:yyyy-MM-dd}",
                    EffectiveDate = dayRates.Date
                };

                // CNB publishes amount units of currency = rate CZK; the database stores PLN per 1 currency.
                table.Rates.Add(CreateRateItem("CZK", 1m / plnToCzk));

                foreach (var currencyCode in SupportedCurrencyCodes.OrderBy(code => code))
                {
                    if (currencyCode is "CZK" or "PLN" ||
                        !dayRates.Rates.TryGetValue(currencyCode, out var publishedRate))
                    {
                        continue;
                    }

                    var currencyToCzk = publishedRate.Rate / publishedRate.Amount;
                    table.Rates.Add(CreateRateItem(currencyCode, currencyToCzk / plnToCzk));
                }

                tables.Add(table);
            }

            return tables;
        }

        private static InvalidOperationException CreateMissingPlnException(DateTime effectiveDate)
        {
            return new InvalidOperationException(
                $"Czech National Bank data for {effectiveDate:yyyy-MM-dd} does not include a usable CZK/PLN rate, so CNB rates cannot be normalized to PLN.");
        }

        private static ExternalExchangeRateItemDto CreateRateItem(string currencyCode, decimal rateToPln)
        {
            var roundedRate = decimal.Round(rateToPln, 6, MidpointRounding.AwayFromZero);

            return new ExternalExchangeRateItemDto
            {
                CurrencyCode = currencyCode,
                CurrencyName = currencyCode,
                BuyPrice = roundedRate,
                SellPrice = roundedRate,
                ReferenceRate = roundedRate
            };
        }

        private static bool TryGetString(JsonElement element, string propertyName, out string value)
        {
            value = string.Empty;
            if (!element.TryGetProperty(propertyName, out var property) ||
                property.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            value = property.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool TryGetPositiveInt(JsonElement element, string propertyName, out int value)
        {
            value = 0;
            return element.TryGetProperty(propertyName, out var property) &&
                property.TryGetInt32(out value) &&
                value > 0;
        }

        private static bool TryGetPositiveDecimal(JsonElement element, string propertyName, out decimal value)
        {
            value = 0m;
            return element.TryGetProperty(propertyName, out var property) &&
                property.TryGetDecimal(out value) &&
                value > 0;
        }
    }

    public readonly record struct CnbPublishedRate(DateTime ValidFor, string CurrencyCode, int Amount, decimal Rate);
}
