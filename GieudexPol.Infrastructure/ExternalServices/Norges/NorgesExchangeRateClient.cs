using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.Json;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;

namespace GieudexPol.Infrastructure.ExternalServices.Norges
{
    public class NorgesExchangeRateClient : IExternalExchangeRateClient
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

        private static readonly IReadOnlyList<string> RequestedCurrencyCodes =
            SupportedCurrencyCodes
                .Where(code => !string.Equals(code, "NOK", StringComparison.OrdinalIgnoreCase))
                .OrderBy(code => code)
                .ToArray();

        private readonly HttpClient _httpClient;

        public string SourceCode => "NORGES";

        public string SourceName => "Norges Bank";

        public int MaxRangeDays => 366;

        public NorgesExchangeRateClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var observations = new ConcurrentBag<NorgesObservation>();

            await Parallel.ForEachAsync(
                RequestedCurrencyCodes,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 4,
                    CancellationToken = cancellationToken
                },
                async (currencyCode, token) =>
                {
                    var endpoint =
                        $"data/EXR/B.{currencyCode}.NOK.SP?format=sdmx-json" +
                        $"&startPeriod={from:yyyy-MM-dd}&endPeriod={to:yyyy-MM-dd}&locale=en";

                    using var response = await _httpClient.GetAsync(endpoint, token);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return;
                    }

                    response.EnsureSuccessStatusCode();

                    await using var stream = await response.Content.ReadAsStreamAsync(token);
                    using var document = await JsonDocument.ParseAsync(stream, cancellationToken: token);

                    foreach (var observation in ParseDailyObservations(document.RootElement))
                    {
                        observations.Add(observation);
                    }
                });

            return ConvertObservationsToPlnRates(observations, from.Date, to.Date);
        }

        public static IReadOnlyList<NorgesObservation> ParseDailyObservations(JsonElement root)
        {
            var observations = new List<NorgesObservation>();
            if (!TryGetDataElements(root, out var dataSet, out var structure) ||
                !TryGetArray(structure, "dimensions", "series", out var seriesDimensions) ||
                !TryGetArray(structure, "attributes", "series", out var seriesAttributes) ||
                !TryGetObject(dataSet, "series", out var seriesObject) ||
                !TryFindElementIndex(seriesDimensions, "BASE_CUR", out var baseCurrencyDimensionIndex) ||
                !TryFindElementIndex(seriesAttributes, "UNIT_MULT", out var unitMultiplierAttributeIndex) ||
                !TryGetObservationDates(structure, out var dates))
            {
                return observations;
            }

            var baseCurrencyValues = GetValues(seriesDimensions[baseCurrencyDimensionIndex]);
            var unitMultiplierValues = GetValues(seriesAttributes[unitMultiplierAttributeIndex]);

            foreach (var series in seriesObject.EnumerateObject())
            {
                var seriesKey = series.Name.Split(':');
                if (baseCurrencyDimensionIndex >= seriesKey.Length ||
                    !int.TryParse(seriesKey[baseCurrencyDimensionIndex], out var currencyIndex) ||
                    currencyIndex < 0 ||
                    currencyIndex >= baseCurrencyValues.Count)
                {
                    continue;
                }

                var currencyCode = baseCurrencyValues[currencyIndex].Trim().ToUpperInvariant();
                if (!SupportedCurrencyCodes.Contains(currencyCode) ||
                    !TryGetSeriesUnit(
                        series.Value,
                        unitMultiplierAttributeIndex,
                        unitMultiplierValues,
                        out var unit) ||
                    !series.Value.TryGetProperty("observations", out var observationElements) ||
                    observationElements.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (var observation in observationElements.EnumerateObject())
                {
                    if (!int.TryParse(observation.Name, out var dateIndex) ||
                        dateIndex < 0 ||
                        dateIndex >= dates.Count ||
                        !TryGetObservationValue(observation.Value, out var nokRate))
                    {
                        continue;
                    }

                    observations.Add(new NorgesObservation(currencyCode, dates[dateIndex], unit, nokRate));
                }
            }

            return observations;
        }

        public static IReadOnlyList<ExternalExchangeRateTableDto> ConvertObservationsToPlnRates(
            IEnumerable<NorgesObservation> observations,
            DateTime from,
            DateTime to)
        {
            var ratesByDate = observations
                .Where(observation =>
                    observation.Date.Date >= from.Date &&
                    observation.Date.Date <= to.Date &&
                    observation.Unit > 0 &&
                    observation.RateInNok > 0 &&
                    SupportedCurrencyCodes.Contains(observation.CurrencyCode))
                .GroupBy(observation => observation.Date.Date)
                .OrderBy(group => group.Key)
                .Select(group => new
                {
                    Date = group.Key,
                    Rates = group
                        .GroupBy(observation => observation.CurrencyCode, StringComparer.OrdinalIgnoreCase)
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

                var plnToNok = plnRate.RateInNok / plnRate.Unit;
                if (plnToNok <= 0)
                {
                    throw CreateMissingPlnException(dayRates.Date);
                }

                var table = new ExternalExchangeRateTableDto
                {
                    Table = "NORGES",
                    Number = $"NORGES/{dayRates.Date:yyyy-MM-dd}",
                    EffectiveDate = dayRates.Date
                };

                // Norges Bank quotes Unit units of BASE_CUR in NOK. Store PLN per 1 unit of currency.
                // Norges Bank rates are normalized to PLN before storing because the application stores PLN per 1 unit of currency.
                table.Rates.Add(CreateRateItem("NOK", 1m / plnToNok));

                foreach (var currencyCode in SupportedCurrencyCodes.OrderBy(code => code))
                {
                    if (currencyCode is "NOK" or "PLN" ||
                        !dayRates.Rates.TryGetValue(currencyCode, out var observation))
                    {
                        continue;
                    }

                    var currencyToNok = observation.RateInNok / observation.Unit;
                    table.Rates.Add(CreateRateItem(currencyCode, currencyToNok / plnToNok));
                }

                tables.Add(table);
            }

            return tables;
        }

        private static bool TryGetDataElements(
            JsonElement root,
            out JsonElement dataSet,
            out JsonElement structure)
        {
            dataSet = default;
            structure = default;

            return root.TryGetProperty("data", out var data) &&
                data.TryGetProperty("dataSets", out var dataSets) &&
                dataSets.ValueKind == JsonValueKind.Array &&
                dataSets.GetArrayLength() > 0 &&
                (dataSet = dataSets[0]).ValueKind == JsonValueKind.Object &&
                data.TryGetProperty("structure", out structure) &&
                structure.ValueKind == JsonValueKind.Object;
        }

        private static bool TryGetArray(
            JsonElement element,
            string objectPropertyName,
            string arrayPropertyName,
            out JsonElement[] values)
        {
            values = [];
            if (!element.TryGetProperty(objectPropertyName, out var objectElement) ||
                !objectElement.TryGetProperty(arrayPropertyName, out var arrayElement) ||
                arrayElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            values = arrayElement.EnumerateArray().ToArray();
            return true;
        }

        private static bool TryGetObject(JsonElement element, string propertyName, out JsonElement value)
        {
            value = default;
            return element.TryGetProperty(propertyName, out value) &&
                value.ValueKind == JsonValueKind.Object;
        }

        private static bool TryFindElementIndex(
            IReadOnlyList<JsonElement> elements,
            string id,
            out int index)
        {
            for (var candidateIndex = 0; candidateIndex < elements.Count; candidateIndex++)
            {
                if (elements[candidateIndex].TryGetProperty("id", out var idElement) &&
                    string.Equals(idElement.GetString(), id, StringComparison.OrdinalIgnoreCase))
                {
                    index = candidateIndex;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private static IReadOnlyList<string> GetValues(JsonElement dimensionOrAttribute)
        {
            if (!dimensionOrAttribute.TryGetProperty("values", out var values) ||
                values.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return values.EnumerateArray()
                .Select(value => value.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty)
                .ToArray();
        }

        private static bool TryGetObservationDates(JsonElement structure, out IReadOnlyList<DateTime> dates)
        {
            dates = [];
            if (!TryGetArray(structure, "dimensions", "observation", out var observationDimensions) ||
                !TryFindElementIndex(observationDimensions, "TIME_PERIOD", out var timeDimensionIndex))
            {
                return false;
            }

            var dateValues = GetValues(observationDimensions[timeDimensionIndex]);
            var parsedDates = new List<DateTime>();
            foreach (var dateValue in dateValues)
            {
                if (!DateTime.TryParse(
                        dateValue,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal,
                        out var date))
                {
                    return false;
                }

                parsedDates.Add(date.Date);
            }

            dates = parsedDates;
            return dates.Count > 0;
        }

        private static bool TryGetSeriesUnit(
            JsonElement series,
            int unitMultiplierAttributeIndex,
            IReadOnlyList<string> unitMultiplierValues,
            out int unit)
        {
            unit = 0;
            if (!series.TryGetProperty("attributes", out var attributes) ||
                attributes.ValueKind != JsonValueKind.Array ||
                unitMultiplierAttributeIndex >= attributes.GetArrayLength() ||
                !attributes[unitMultiplierAttributeIndex].TryGetInt32(out var multiplierValueIndex) ||
                multiplierValueIndex < 0 ||
                multiplierValueIndex >= unitMultiplierValues.Count ||
                !int.TryParse(unitMultiplierValues[multiplierValueIndex], out var exponent) ||
                exponent < 0 ||
                exponent > 6)
            {
                return false;
            }

            unit = 1;
            for (var index = 0; index < exponent; index++)
            {
                unit *= 10;
            }

            return true;
        }

        private static bool TryGetObservationValue(JsonElement observation, out decimal value)
        {
            value = 0m;
            if (observation.ValueKind != JsonValueKind.Array ||
                observation.GetArrayLength() == 0)
            {
                return false;
            }

            var valueElement = observation[0];
            if (valueElement.ValueKind == JsonValueKind.Number)
            {
                return valueElement.TryGetDecimal(out value) && value > 0;
            }

            return valueElement.ValueKind == JsonValueKind.String &&
                decimal.TryParse(
                    valueElement.GetString(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out value) &&
                value > 0;
        }

        private static InvalidOperationException CreateMissingPlnException(DateTime effectiveDate)
        {
            return new InvalidOperationException(
                $"Norges Bank data for {effectiveDate:yyyy-MM-dd} does not include a usable NOK/PLN rate, so NORGES rates cannot be normalized to PLN.");
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

    public readonly record struct NorgesObservation(string CurrencyCode, DateTime Date, int Unit, decimal RateInNok);
}
