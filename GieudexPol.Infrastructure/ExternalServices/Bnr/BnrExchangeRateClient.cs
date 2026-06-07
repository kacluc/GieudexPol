using System.Globalization;
using System.Net;
using System.Xml.Linq;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;

namespace GieudexPol.Infrastructure.ExternalServices.Bnr
{
    public class BnrExchangeRateClient : IExternalExchangeRateClient
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

        public string SourceCode => "BNR";

        public string SourceName => "National Bank of Romania";

        public int MaxRangeDays => 366;

        public BnrExchangeRateClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var tables = new List<ExternalExchangeRateTableDto>();

            for (var year = from.Year; year <= to.Year; year++)
            {
                var endpoint = $"files/xml/years/nbrfxrates{year}.xml";
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }

                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
                tables.AddRange(ParsePublishedRates(document, from.Date, to.Date));
            }

            return tables
                .GroupBy(table => table.EffectiveDate.Date)
                .Select(group => group.Last())
                .OrderBy(table => table.EffectiveDate)
                .ToList();
        }

        public static IReadOnlyList<ExternalExchangeRateTableDto> ParsePublishedRates(
            XDocument document,
            DateTime from,
            DateTime to)
        {
            var tables = new List<ExternalExchangeRateTableDto>();

            foreach (var cube in document
                .Descendants()
                .Where(element => element.Name.LocalName == "Cube"))
            {
                if (!DateTime.TryParse(
                        cube.Attribute("date")?.Value,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal,
                        out var effectiveDate))
                {
                    continue;
                }

                effectiveDate = effectiveDate.Date;
                if (effectiveDate < from.Date || effectiveDate > to.Date)
                {
                    continue;
                }

                var rates = cube
                    .Elements()
                    .Where(element => element.Name.LocalName == "Rate")
                    .Select(ParseRate)
                    .Where(rate => rate.HasValue)
                    .Select(rate => rate!.Value)
                    .Where(rate => SupportedCurrencyCodes.Contains(rate.CurrencyCode))
                    .GroupBy(rate => rate.CurrencyCode, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Last(),
                        StringComparer.OrdinalIgnoreCase);

                if (!rates.TryGetValue("PLN", out var plnRate))
                {
                    throw CreateMissingPlnException(effectiveDate);
                }

                var plnToRon = plnRate.RateInRon / plnRate.Multiplier;
                if (plnToRon <= 0)
                {
                    throw CreateMissingPlnException(effectiveDate);
                }

                var table = new ExternalExchangeRateTableDto
                {
                    Table = "BNR",
                    Number = $"BNR/{effectiveDate:yyyy-MM-dd}",
                    EffectiveDate = effectiveDate
                };

                // BNR returns rates as multiplier units of currency = rate RON. The application stores values as PLN per 1 unit of currency.
                table.Rates.Add(CreateRateItem("RON", 1m / plnToRon));

                foreach (var currencyCode in SupportedCurrencyCodes.OrderBy(code => code))
                {
                    if (currencyCode is "PLN" or "RON" ||
                        !rates.TryGetValue(currencyCode, out var publishedRate))
                    {
                        continue;
                    }

                    var currencyToRon = publishedRate.RateInRon / publishedRate.Multiplier;
                    table.Rates.Add(CreateRateItem(currencyCode, currencyToRon / plnToRon));
                }

                tables.Add(table);
            }

            return tables
                .OrderBy(table => table.EffectiveDate)
                .ToList();
        }

        private static BnrPublishedRate? ParseRate(XElement element)
        {
            var currencyCode = element.Attribute("currency")?.Value?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(currencyCode) ||
                !decimal.TryParse(element.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var rateInRon) ||
                rateInRon <= 0)
            {
                return null;
            }

            var multiplierText = element.Attribute("multiplier")?.Value;
            var multiplier = 1;
            if (!string.IsNullOrWhiteSpace(multiplierText) &&
                (!int.TryParse(multiplierText, NumberStyles.Integer, CultureInfo.InvariantCulture, out multiplier) ||
                 multiplier <= 0))
            {
                return null;
            }

            return new BnrPublishedRate(currencyCode, multiplier, rateInRon);
        }

        private static InvalidOperationException CreateMissingPlnException(DateTime effectiveDate)
        {
            return new InvalidOperationException(
                $"National Bank of Romania data for {effectiveDate:yyyy-MM-dd} does not include a usable RON/PLN rate, so BNR rates cannot be normalized to PLN.");
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
    }

    public readonly record struct BnrPublishedRate(string CurrencyCode, int Multiplier, decimal RateInRon);
}
