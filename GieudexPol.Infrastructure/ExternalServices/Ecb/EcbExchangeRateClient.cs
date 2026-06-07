using System.Globalization;
using System.Xml.Linq;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;

namespace GieudexPol.Infrastructure.ExternalServices.Ecb
{
    public class EcbExchangeRateClient : IExternalExchangeRateClient
    {
        private const string HistoricalRatesEndpoint = "eurofxref-hist.xml";

        private readonly HttpClient _httpClient;

        public string SourceCode => "ECB";
        public string SourceName => "European Central Bank";
        public int MaxRangeDays => 366;

        public EcbExchangeRateClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(HistoricalRatesEndpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

            return ParseHistoricalRates(document, from.Date, to.Date);
        }

        public static IReadOnlyList<ExternalExchangeRateTableDto> ParseHistoricalRates(
            XDocument document,
            DateTime from,
            DateTime to)
        {
            var tables = new List<ExternalExchangeRateTableDto>();
            var dayCubes = document
                .Descendants()
                .Where(element =>
                    element.Name.LocalName == "Cube" &&
                    element.Attribute("time") != null);

            foreach (var dayCube in dayCubes)
            {
                if (!DateTime.TryParse(
                        dayCube.Attribute("time")?.Value,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal,
                        out var effectiveDate))
                {
                    continue;
                }

                effectiveDate = effectiveDate.Date;
                if (effectiveDate < from.Date || effectiveDate > to.Date)
                {
                    continue;
                }

                var rates = dayCube
                    .Elements()
                    .Where(element => element.Name.LocalName == "Cube")
                    .Select(element => new
                    {
                        Currency = element.Attribute("currency")?.Value?.Trim().ToUpperInvariant(),
                        RateText = element.Attribute("rate")?.Value
                    })
                    .Where(rate => !string.IsNullOrWhiteSpace(rate.Currency))
                    .Select(rate => new
                    {
                        Currency = rate.Currency!,
                        Parsed = decimal.TryParse(
                            rate.RateText,
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out var value)
                            ? value
                            : (decimal?)null
                    })
                    .Where(rate => rate.Parsed.HasValue)
                    .ToDictionary(rate => rate.Currency, rate => rate.Parsed!.Value);

                if (!rates.TryGetValue("PLN", out var eurPlnRate) || eurPlnRate <= 0)
                {
                    continue;
                }

                var table = new ExternalExchangeRateTableDto
                {
                    Table = "ECB",
                    Number = $"ECB/{effectiveDate:yyyy-MM-dd}",
                    EffectiveDate = effectiveDate
                };

                table.Rates.Add(CreateRateItem("EUR", eurPlnRate));

                foreach (var (currencyCode, eurCurrencyRate) in rates.OrderBy(rate => rate.Key))
                {
                    if (currencyCode == "PLN" || eurCurrencyRate <= 0)
                    {
                        continue;
                    }

                    var rateToPln = eurPlnRate / eurCurrencyRate;
                    table.Rates.Add(CreateRateItem(currencyCode, rateToPln));
                }

                tables.Add(table);
            }

            return tables
                .OrderBy(table => table.EffectiveDate)
                .ToList();
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
}
