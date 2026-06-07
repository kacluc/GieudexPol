using System.Globalization;
using System.Text;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;

namespace GieudexPol.Infrastructure.ExternalServices.BankOfEngland
{
    public class BankOfEnglandExchangeRateClient : IExternalExchangeRateClient
    {
        private const string CsvExportEndpoint = "_iadb-fromshowcolumns.asp";

        // Daily published BoE Database spot series whose values are units of currency per GBP 1.
        private static readonly IReadOnlyDictionary<string, string> SupportedCurrencySeries = new Dictionary<string, string>
        {
            ["AUD"] = "XUDLADS",
            ["CAD"] = "XUDLCDS",
            ["CHF"] = "XUDLSFS",
            ["CZK"] = "XUDLBK25",
            ["DKK"] = "XUDLDKS",
            ["EUR"] = "XUDLERS",
            ["HUF"] = "XUDLBK33",
            ["JPY"] = "XUDLJYS",
            ["KRW"] = "XUDLBK93",
            ["NOK"] = "XUDLNKS",
            ["PLN"] = "XUDLBK47",
            ["RON"] = "XUDLZOS4",
            ["SEK"] = "XUDLSKS",
            ["TRY"] = "XUDLBK95",
            ["USD"] = "XUDLUSS"
        };

        private static readonly IReadOnlyDictionary<string, string> SeriesCurrencyCodes =
            SupportedCurrencySeries.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

        private readonly HttpClient _httpClient;

        public string SourceCode => "BOE";
        public string SourceName => "Bank of England";
        public int MaxRangeDays => 366;

        public BankOfEnglandExchangeRateClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var seriesCodes = string.Join(",", SupportedCurrencySeries.Values);
            var endpoint =
                $"{CsvExportEndpoint}?csv.x=yes" +
                $"&Datefrom={Uri.EscapeDataString(from.ToString("dd/MMM/yyyy", CultureInfo.InvariantCulture))}" +
                $"&Dateto={Uri.EscapeDataString(to.ToString("dd/MMM/yyyy", CultureInfo.InvariantCulture))}" +
                $"&SeriesCodes={Uri.EscapeDataString(seriesCodes)}" +
                "&CSVF=TN&UsingCodes=Y&VPD=Y&VFD=N";

            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var csv = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParsePublishedSpotRates(csv, from.Date, to.Date);
        }

        public static IReadOnlyList<ExternalExchangeRateTableDto> ParsePublishedSpotRates(
            string csv,
            DateTime from,
            DateTime to)
        {
            using var reader = new StringReader(csv);
            var headerLine = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return Array.Empty<ExternalExchangeRateTableDto>();
            }

            var header = ParseCsvLine(headerLine);
            var columns = header
                .Select((value, index) => new { SeriesCode = value.Trim().TrimStart('\uFEFF'), Index = index })
                .Where(column => SeriesCurrencyCodes.ContainsKey(column.SeriesCode))
                .ToDictionary(
                    column => column.Index,
                    column => SeriesCurrencyCodes[column.SeriesCode]);

            if (!columns.Values.Contains("PLN", StringComparer.OrdinalIgnoreCase))
            {
                throw CreateMissingPlnException();
            }

            var tables = new List<ExternalExchangeRateTableDto>();
            var hasPublishedRowsInRange = false;

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = ParseCsvLine(line);
                if (values.Count == 0 ||
                    !DateTime.TryParse(
                        values[0],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces,
                        out var effectiveDate))
                {
                    continue;
                }

                effectiveDate = effectiveDate.Date;
                if (effectiveDate < from.Date || effectiveDate > to.Date)
                {
                    continue;
                }

                hasPublishedRowsInRange = true;
                var publishedRates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                foreach (var (index, currencyCode) in columns)
                {
                    if (index >= values.Count ||
                        !decimal.TryParse(values[index], NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ||
                        value <= 0)
                    {
                        continue;
                    }

                    publishedRates[currencyCode] = value;
                }

                if (!publishedRates.TryGetValue("PLN", out var gbpPlnRate))
                {
                    continue;
                }

                var table = new ExternalExchangeRateTableDto
                {
                    Table = "BOE",
                    Number = $"BOE/{effectiveDate:yyyy-MM-dd}",
                    EffectiveDate = effectiveDate
                };

                table.Rates.Add(CreateRateItem("GBP", gbpPlnRate));

                foreach (var currencyCode in SupportedCurrencySeries.Keys.OrderBy(code => code))
                {
                    if (currencyCode == "PLN" ||
                        !publishedRates.TryGetValue(currencyCode, out var gbpCurrencyRate))
                    {
                        continue;
                    }

                    // BoE values are currency units per GBP 1; PLN per currency = GBP_PLN / GBP_CURRENCY.
                    table.Rates.Add(CreateRateItem(currencyCode, gbpPlnRate / gbpCurrencyRate));
                }

                tables.Add(table);
            }

            if (tables.Count == 0 && hasPublishedRowsInRange)
            {
                throw CreateMissingPlnException();
            }

            return tables;
        }

        private static InvalidOperationException CreateMissingPlnException()
        {
            return new InvalidOperationException(
                "Bank of England data does not include a usable published GBP/PLN spot rate, so BOE rates cannot be normalized to PLN.");
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

        private static IReadOnlyList<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            var insideQuotes = false;

            for (var index = 0; index < line.Length; index++)
            {
                var character = line[index];
                if (character == '"')
                {
                    if (insideQuotes && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        currentValue.Append('"');
                        index++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }

                    continue;
                }

                if (character == ',' && !insideQuotes)
                {
                    values.Add(currentValue.ToString().Trim());
                    currentValue.Clear();
                    continue;
                }

                currentValue.Append(character);
            }

            values.Add(currentValue.ToString().Trim());
            return values;
        }
    }
}
