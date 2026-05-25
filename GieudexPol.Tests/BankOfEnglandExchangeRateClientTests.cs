using FluentAssertions;
using GieudexPol.Infrastructure.ExternalServices.BankOfEngland;

namespace GieudexPol.Tests
{
    public class BankOfEnglandExchangeRateClientTests
    {
        [Fact]
        public void ParsePublishedSpotRates_ShouldConvertUsdRateToPln()
        {
            const string csv = """
                DATE,XUDLBK47,XUDLUSS
                02 Jan 2026,5.10,1.25
                """;

            var tables = BankOfEnglandExchangeRateClient.ParsePublishedSpotRates(
                csv,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var usdRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "USD");
            usdRate.BuyPrice.Should().Be(decimal.Round(5.10m / 1.25m, 6, MidpointRounding.AwayFromZero));
            usdRate.SellPrice.Should().Be(usdRate.BuyPrice);
        }

        [Fact]
        public void ParsePublishedSpotRates_ShouldUsePublishedPlnRateForGbp()
        {
            const string csv = """
                DATE,XUDLBK47,XUDLERS
                02 Jan 2026,5.10,1.18
                """;

            var tables = BankOfEnglandExchangeRateClient.ParsePublishedSpotRates(
                csv,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var gbpRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "GBP");
            gbpRate.BuyPrice.Should().Be(5.10m);
            gbpRate.SellPrice.Should().Be(5.10m);
        }

        [Fact]
        public void ParsePublishedSpotRates_ShouldFailClearlyWhenPublishedDayHasNoPlnRate()
        {
            const string csv = """
                DATE,XUDLBK47,XUDLUSS
                02 Jan 2026,,1.25
                """;

            var action = () => BankOfEnglandExchangeRateClient.ParsePublishedSpotRates(
                csv,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*GBP/PLN*normalized to PLN*");
        }
    }
}
