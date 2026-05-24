using System.Globalization;
using System.Net;
using System.Text.Json;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;

namespace GieudexPol.Infrastructure.ExternalServices.Riksbank
{
    public class RiksbankExchangeRateClient : IExternalExchangeRateClient
    {
        private const int CurrencyGroupId = 130;

        private static readonly IReadOnlyDictionary<string, string> SupportedCurrencySeries = new Dictionary<string, string>
        {
            ["AUD"] = "SEKAUDPMI",
            ["CAD"] = "SEKCADPMI",
            ["CHF"] = "SEKCHFPMI",
            ["CZK"] = "SEKCZKPMI",
            ["DKK"] = "SEKDKKPMI",
            ["EUR"] = "SEKEURPMI",
            ["GBP"] = "SEKGBPPMI",
            ["HUF"] = "SEKHUFPMI",
            ["JPY"] = "SEKJPYPMI",
            ["KRW"] = "SEKKRWPMI",
            ["NOK"] = "SEKNOKPMI",
            ["PLN"] = "SEKPLNPMI",
            ["RON"] = "SEKRONPMI",
            ["TRY"] = "SEKTRYPMI",
            ["USD"] = "SEKUSDPMI"
        };

        private static readonly IReadOnlyDictionary<string, string> SeriesCurrencyCodes =
            SupportedCurrencySeries.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

        private readonly HttpClient _httpClient;

        public string SourceCode => "RIKSBANK";
        public string SourceName => "Sveriges Riksbank";
        public int MaxRangeDays => 366;

        public RiksbankExchangeRateClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var observations = await GetGroupObservationsAsync(from.Date, to.Date, cancellationToken);
            return ConvertObservationsToPlnRates(observations, from.Date, to.Date);
        }

        public static IReadOnlyList<ExternalExchangeRateTableDto> ConvertObservationsToPlnRates(
            IEnumerable<RiksbankObservation> observations,
            DateTime from,
            DateTime to)
        {
            var ratesByDate = observations
                .Where(observation =>
                    observation.Date.Date >= from.Date &&
                    observation.Date.Date <= to.Date &&
                    observation.Value > 0 &&
                    SeriesCurrencyCodes.ContainsKey(observation.SeriesId))
                .GroupBy(observation => observation.Date.Date)
                .OrderBy(group => group.Key)
                .Select(group => new
                {
                    Date = group.Key,
                    Rates = group
                        .GroupBy(observation => SeriesCurrencyCodes[observation.SeriesId])
                        .ToDictionary(
                            currencyGroup => currencyGroup.Key,
                            currencyGroup => currencyGroup.Last().Value)
                });

            var tables = new List<ExternalExchangeRateTableDto>();

            foreach (var dayRates in ratesByDate)
            {
                if (!dayRates.Rates.TryGetValue("PLN", out var sekPerPln) || sekPerPln <= 0)
                {
                    continue;
                }

                var table = new ExternalExchangeRateTableDto
                {
                    Table = "RIKSBANK",
                    Number = $"RIKSBANK/{dayRates.Date:yyyy-MM-dd}",
                    EffectiveDate = dayRates.Date
                };

                table.Rates.Add(CreateRateItem("SEK", 1m / sekPerPln));

                foreach (var currencyCode in SupportedCurrencySeries.Keys.OrderBy(code => code))
                {
                    if (currencyCode is "PLN" or "SEK")
                    {
                        continue;
                    }

                    if (!dayRates.Rates.TryGetValue(currencyCode, out var sekPerCurrency) || sekPerCurrency <= 0)
                    {
                        continue;
                    }

                    var rateToPln = sekPerCurrency / sekPerPln;
                    table.Rates.Add(CreateRateItem(currencyCode, rateToPln));
                }

                tables.Add(table);
            }

            return tables;
        }

        private async Task<IReadOnlyList<RiksbankObservation>> GetGroupObservationsAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken)
        {
            var endpoint = $"Observations/ByGroup/{CurrencyGroupId}/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}";
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Array.Empty<RiksbankObservation>();
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return ParseGroupObservations(document.RootElement);
        }

        public static IReadOnlyList<RiksbankObservation> ParseGroupObservations(JsonElement root)
        {
            var observations = new List<RiksbankObservation>();
            CollectObservations(root, observations, currentSeriesId: null);
            return observations;
        }

        private static void CollectObservations(
            JsonElement element,
            ICollection<RiksbankObservation> observations,
            string? currentSeriesId)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (TryGetString(element, "seriesId", out var elementSeriesId))
                {
                    currentSeriesId = elementSeriesId;
                }

                if (TryParseObservation(element, currentSeriesId, out var observation))
                {
                    observations.Add(observation);
                }

                foreach (var property in element.EnumerateObject())
                {
                    CollectObservations(property.Value, observations, currentSeriesId);
                }

                return;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    CollectObservations(item, observations, currentSeriesId);
                }
            }
        }

        private static bool TryParseObservation(JsonElement element, string? fallbackSeriesId, out RiksbankObservation observation)
        {
            observation = default;

            if (!TryGetString(element, "seriesId", out var seriesId))
            {
                seriesId = fallbackSeriesId ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(seriesId) ||
                !TryGetString(element, "date", out var dateText) ||
                !TryGetDecimal(element, "value", out var value))
            {
                return false;
            }

            if (!DateTime.TryParse(
                    dateText,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var date))
            {
                return false;
            }

            observation = new RiksbankObservation(seriesId.Trim().ToUpperInvariant(), date.Date, value);
            return true;
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

        private static bool TryGetDecimal(JsonElement element, string propertyName, out decimal value)
        {
            value = 0m;

            if (!element.TryGetProperty(propertyName, out var property))
            {
                return false;
            }

            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.TryGetDecimal(out value);
            }

            if (property.ValueKind == JsonValueKind.String)
            {
                return decimal.TryParse(
                    property.GetString(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            return false;
        }

        private static ExternalExchangeRateItemDto CreateRateItem(string currencyCode, decimal rateToPln)
        {
            var roundedRate = decimal.Round(rateToPln, 6, MidpointRounding.AwayFromZero);

            return new ExternalExchangeRateItemDto
            {
                CurrencyCode = currencyCode,
                CurrencyName = currencyCode,
                BuyPrice = roundedRate,
                SellPrice = roundedRate
            };
        }
    }

    public readonly record struct RiksbankObservation(string SeriesId, DateTime Date, decimal Value);
}
