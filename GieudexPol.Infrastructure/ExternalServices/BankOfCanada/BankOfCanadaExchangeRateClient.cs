using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;

namespace GieudexPol.Infrastructure.ExternalServices.BankOfCanada
{
    public class BankOfCanadaExchangeRateClient : IExternalExchangeRateClient
    {
        private const string ObservationsEndpoint = "observations";
        private const string PlnSeriesCode = "FXPLNCAD";

        private static readonly IReadOnlyDictionary<string, string> CurrencySeries =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["USD"] = "FXUSDCAD",
                ["EUR"] = "FXEURCAD",
                ["GBP"] = "FXGBPCAD",
                ["CHF"] = "FXCHFCAD",
                ["JPY"] = "FXJPYCAD"
            };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public string SourceCode => "BOC";
        public string SourceName => "Bank of Canada";
        public int MaxRangeDays => 366;

        public BankOfCanadaExchangeRateClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var seriesNames = string.Join(",", CurrencySeries.Values.Append(PlnSeriesCode));
            var endpoint =
                $"{ObservationsEndpoint}/{seriesNames}/json" +
                $"?start_date={from:yyyy-MM-dd}&end_date={to:yyyy-MM-dd}";

            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseObservations(json, from.Date, to.Date);
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetLatestExchangeRatesAsync(
            CancellationToken cancellationToken = default)
        {
            var to = DateTime.Today;
            var from = to.AddDays(-14);
            var tables = await GetBuySellRatesAsync(from, to, cancellationToken);

            return tables.Count == 0
                ? []
                : [tables.OrderByDescending(table => table.EffectiveDate).First()];
        }

        public static IReadOnlyList<ExternalExchangeRateTableDto> ParseObservations(
            string json,
            DateTime from,
            DateTime to)
        {
            var response = JsonSerializer.Deserialize<BankOfCanadaResponseDto>(json, JsonOptions);
            if (response?.Observations == null)
            {
                return [];
            }

            var tables = new List<ExternalExchangeRateTableDto>();
            foreach (var observation in response.Observations)
            {
                if (!DateTime.TryParseExact(
                        observation.Date,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var effectiveDate) ||
                    effectiveDate.Date < from.Date ||
                    effectiveDate.Date > to.Date ||
                    !TryGetPositiveDecimal(observation.PlnCad, out var plnCadRate))
                {
                    continue;
                }

                var table = new ExternalExchangeRateTableDto
                {
                    Table = SourceCodeValue,
                    Number = $"{SourceCodeValue}/{effectiveDate:yyyy-MM-dd}",
                    EffectiveDate = effectiveDate.Date
                };

                // Bank of Canada publishes reference FX rates, not official bid/ask tables.
                // MidPrice is converted to PLN using CAD cross-rates.
                // BuyPrice and SellPrice are synthetic values calculated from configured spread.
                table.Rates.Add(CreateRateItem("CAD", CalculateCadRateToPln(plnCadRate)));

                foreach (var (currencyCode, seriesCode) in CurrencySeries)
                {
                    if (!observation.SeriesValues.TryGetValue(seriesCode, out var seriesValue) ||
                        !TryGetPositiveDecimal(seriesValue, out var currencyCadRate))
                    {
                        continue;
                    }

                    table.Rates.Add(
                        CreateRateItem(
                            currencyCode,
                            CalculateCrossRateToPln(currencyCadRate, plnCadRate)));
                }

                if (table.Rates.Count > 0)
                {
                    tables.Add(table);
                }
            }

            return tables
                .OrderBy(table => table.EffectiveDate)
                .ToList();
        }

        public static decimal CalculateCrossRateToPln(decimal currencyCadRate, decimal plnCadRate)
        {
            if (currencyCadRate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currencyCadRate),
                    "Currency/CAD rate must be greater than zero.");
            }

            if (plnCadRate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(plnCadRate),
                    "PLN/CAD rate must be greater than zero.");
            }

            return decimal.Round(
                currencyCadRate / plnCadRate,
                4,
                MidpointRounding.AwayFromZero);
        }

        public static decimal CalculateCadRateToPln(decimal plnCadRate)
        {
            return CalculateCrossRateToPln(1m, plnCadRate);
        }

        private const string SourceCodeValue = "BOC";

        private static ExternalExchangeRateItemDto CreateRateItem(
            string currencyCode,
            decimal referenceRate)
        {
            return new ExternalExchangeRateItemDto
            {
                CurrencyCode = currencyCode,
                CurrencyName = currencyCode,
                BuyPrice = referenceRate,
                SellPrice = referenceRate,
                ReferenceRate = referenceRate
            };
        }

        private static bool TryGetPositiveDecimal(
            BankOfCanadaSeriesValueDto? seriesValue,
            out decimal value)
        {
            value = 0m;
            if (seriesValue == null)
            {
                return false;
            }

            return decimal.TryParse(
                       seriesValue.Value,
                       NumberStyles.Number,
                       CultureInfo.InvariantCulture,
                       out value) &&
                   value > 0;
        }
    }

    public sealed class BankOfCanadaResponseDto
    {
        [JsonPropertyName("observations")]
        public List<BankOfCanadaObservationDto> Observations { get; set; } = [];
    }

    public sealed class BankOfCanadaObservationDto
    {
        [JsonPropertyName("d")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("FXPLNCAD")]
        public BankOfCanadaSeriesValueDto? PlnCad { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalValues { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        [JsonIgnore]
        public IReadOnlyDictionary<string, BankOfCanadaSeriesValueDto> SeriesValues =>
            AdditionalValues
                .Where(pair => pair.Value.ValueKind == JsonValueKind.Object)
                .Select(pair => new
                {
                    pair.Key,
                    Value = pair.Value.Deserialize<BankOfCanadaSeriesValueDto>()
                })
                .Where(pair => pair.Value != null)
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value!,
                    StringComparer.OrdinalIgnoreCase);
    }

    public sealed class BankOfCanadaSeriesValueDto
    {
        [JsonPropertyName("v")]
        public string? Value { get; set; }
    }
}
