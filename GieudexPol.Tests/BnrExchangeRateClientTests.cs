using System.Xml.Linq;
using FluentAssertions;
using GieudexPol.Infrastructure.ExternalServices.Bnr;

namespace GieudexPol.Tests
{
    public class BnrExchangeRateClientTests
    {
        [Fact]
        public void ParsePublishedRates_ShouldConvertUsdRateToPln()
        {
            var document = CreateDocument("""
                <Rate currency="PLN">1.08</Rate>
                <Rate currency="USD">4.60</Rate>
                """);

            var tables = BnrExchangeRateClient.ParsePublishedRates(
                document,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var usdRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "USD");
            var expected = decimal.Round(4.60m / 1.08m, 6, MidpointRounding.AwayFromZero);
            usdRate.BuyPrice.Should().Be(expected);
            usdRate.SellPrice.Should().Be(expected);
        }

        [Fact]
        public void ParsePublishedRates_ShouldCreateRonRateFromPlnRate()
        {
            var document = CreateDocument("""<Rate currency="PLN">1.08</Rate>""");

            var tables = BnrExchangeRateClient.ParsePublishedRates(
                document,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var ronRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "RON");
            ronRate.BuyPrice.Should().Be(decimal.Round(1m / 1.08m, 6, MidpointRounding.AwayFromZero));
            ronRate.SellPrice.Should().Be(ronRate.BuyPrice);
        }

        [Fact]
        public void ParsePublishedRates_ShouldRespectMultiplierAndIgnoreUnsupportedCurrency()
        {
            var document = CreateDocument("""
                <Rate currency="PLN">1.08</Rate>
                <Rate currency="JPY" multiplier="100">2.95</Rate>
                <Rate currency="NZD">2.50</Rate>
                """);

            var tables = BnrExchangeRateClient.ParsePublishedRates(
                document,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var table = tables.Single();
            var jpyRate = table.Rates.Single(rate => rate.CurrencyCode == "JPY");
            var expected = decimal.Round((2.95m / 100m) / 1.08m, 6, MidpointRounding.AwayFromZero);
            jpyRate.BuyPrice.Should().Be(expected);
            jpyRate.SellPrice.Should().Be(expected);
            table.Rates.Should().NotContain(rate => rate.CurrencyCode == "NZD");
        }

        [Fact]
        public void ParsePublishedRates_ShouldFailClearlyWhenPublishedDayHasNoPlnRate()
        {
            var document = CreateDocument("""<Rate currency="USD">4.60</Rate>""");

            var action = () => BnrExchangeRateClient.ParsePublishedRates(
                document,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*RON/PLN*normalized to PLN*");
        }

        [Fact]
        public void ParsePublishedRates_ShouldOnlyReturnObservationDatesWithinRange()
        {
            var document = XDocument.Parse("""
                <DataSet xmlns="http://www.bnr.ro/xsd">
                  <Body>
                    <Cube date="2026-01-02">
                      <Rate currency="PLN">1.08</Rate>
                    </Cube>
                    <Cube date="2026-01-05">
                      <Rate currency="PLN">1.09</Rate>
                    </Cube>
                  </Body>
                </DataSet>
                """);

            var tables = BnrExchangeRateClient.ParsePublishedRates(
                document,
                new DateTime(2026, 1, 5),
                new DateTime(2026, 1, 5));

            tables.Should().ContainSingle()
                .Which.EffectiveDate.Should().Be(new DateTime(2026, 1, 5));
        }

        private static XDocument CreateDocument(string rates)
        {
            return XDocument.Parse($$"""
                <DataSet xmlns="http://www.bnr.ro/xsd">
                  <Header>
                    <Publisher>National Bank of Romania</Publisher>
                    <PublishingDate>2026-01-02</PublishingDate>
                  </Header>
                  <Body>
                    <OrigCurrency>RON</OrigCurrency>
                    <Cube date="2026-01-02">
                      {{rates}}
                    </Cube>
                  </Body>
                </DataSet>
                """);
        }
    }
}
