using System.Xml.Linq;
using FluentAssertions;
using GieudexPol.Infrastructure.ExternalServices.Ecb;

namespace GieudexPol.Tests
{
    public class EcbExchangeRateClientTests
    {
        [Fact]
        public void ParseHistoricalRates_ShouldConvertUsdRateToPln()
        {
            var document = XDocument.Parse("""
                <gesmes:Envelope xmlns:gesmes="http://www.gesmes.org/xml/2002-08-01" xmlns="http://www.ecb.int/vocabulary/2002-08-01/eurofxref">
                  <Cube>
                    <Cube time="2026-01-02">
                      <Cube currency="PLN" rate="4.25" />
                      <Cube currency="USD" rate="1.10" />
                    </Cube>
                  </Cube>
                </gesmes:Envelope>
                """);

            var tables = EcbExchangeRateClient.ParseHistoricalRates(
                document,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var usdRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "USD");
            usdRate.BuyPrice.Should().Be(decimal.Round(4.25m / 1.10m, 6, MidpointRounding.AwayFromZero));
            usdRate.SellPrice.Should().Be(usdRate.BuyPrice);
        }

        [Fact]
        public void ParseHistoricalRates_ShouldUseEurPlnRateForEur()
        {
            var document = XDocument.Parse("""
                <gesmes:Envelope xmlns:gesmes="http://www.gesmes.org/xml/2002-08-01" xmlns="http://www.ecb.int/vocabulary/2002-08-01/eurofxref">
                  <Cube>
                    <Cube time="2026-01-02">
                      <Cube currency="PLN" rate="4.25" />
                      <Cube currency="USD" rate="1.10" />
                    </Cube>
                  </Cube>
                </gesmes:Envelope>
                """);

            var tables = EcbExchangeRateClient.ParseHistoricalRates(
                document,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var eurRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "EUR");
            eurRate.BuyPrice.Should().Be(4.25m);
            eurRate.SellPrice.Should().Be(4.25m);
        }

        [Fact]
        public void ParseHistoricalRates_ShouldSkipDayWithoutPlnRate()
        {
            var document = XDocument.Parse("""
                <gesmes:Envelope xmlns:gesmes="http://www.gesmes.org/xml/2002-08-01" xmlns="http://www.ecb.int/vocabulary/2002-08-01/eurofxref">
                  <Cube>
                    <Cube time="2026-01-02">
                      <Cube currency="USD" rate="1.10" />
                    </Cube>
                  </Cube>
                </gesmes:Envelope>
                """);

            var tables = EcbExchangeRateClient.ParseHistoricalRates(
                document,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            tables.Should().BeEmpty();
        }
    }
}
