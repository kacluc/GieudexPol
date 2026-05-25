using System.Text.Json;
using FluentAssertions;
using GieudexPol.Infrastructure.ExternalServices.Cnb;

namespace GieudexPol.Tests
{
    public class CnbExchangeRateClientTests
    {
        [Fact]
        public void ConvertDailyRatesToPlnRates_ShouldConvertUsdRateToPln()
        {
            var rates = new[]
            {
                new CnbPublishedRate(new DateTime(2026, 1, 2), "PLN", 1, 5.50m),
                new CnbPublishedRate(new DateTime(2026, 1, 2), "USD", 1, 22.00m)
            };

            var tables = CnbExchangeRateClient.ConvertDailyRatesToPlnRates(
                rates,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var usdRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "USD");
            usdRate.BuyPrice.Should().Be(4.0m);
            usdRate.SellPrice.Should().Be(usdRate.BuyPrice);
        }

        [Fact]
        public void ConvertDailyRatesToPlnRates_ShouldCreateCzkRateFromPlnRate()
        {
            var rates = new[]
            {
                new CnbPublishedRate(new DateTime(2026, 1, 2), "PLN", 1, 5.50m)
            };

            var tables = CnbExchangeRateClient.ConvertDailyRatesToPlnRates(
                rates,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var czkRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "CZK");
            czkRate.BuyPrice.Should().Be(decimal.Round(1m / 5.50m, 6, MidpointRounding.AwayFromZero));
            czkRate.SellPrice.Should().Be(czkRate.BuyPrice);
        }

        [Fact]
        public void ConvertDailyRatesToPlnRates_ShouldRespectCurrencyAmount()
        {
            var rates = new[]
            {
                new CnbPublishedRate(new DateTime(2026, 1, 2), "PLN", 1, 5.50m),
                new CnbPublishedRate(new DateTime(2026, 1, 2), "JPY", 100, 15.80m)
            };

            var tables = CnbExchangeRateClient.ConvertDailyRatesToPlnRates(
                rates,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var jpyRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "JPY");
            var expected = decimal.Round((15.80m / 100m) / 5.50m, 6, MidpointRounding.AwayFromZero);
            jpyRate.BuyPrice.Should().Be(expected);
            jpyRate.SellPrice.Should().Be(expected);
        }

        [Fact]
        public void ConvertDailyRatesToPlnRates_ShouldFailClearlyWhenPublishedDayHasNoPlnRate()
        {
            var rates = new[]
            {
                new CnbPublishedRate(new DateTime(2026, 1, 2), "USD", 1, 22.00m)
            };

            var action = () => CnbExchangeRateClient.ConvertDailyRatesToPlnRates(
                rates,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*CZK/PLN*normalized to PLN*");
        }

        [Fact]
        public void ParseDailyRates_ShouldReadOfficialJsonFieldsAndIgnoreUnsupportedCurrency()
        {
            using var document = JsonDocument.Parse("""
                {
                  "rates": [
                    { "validFor": "2026-01-02", "amount": 1, "currencyCode": "PLN", "rate": 5.500 },
                    { "validFor": "2026-01-02", "amount": 100, "currencyCode": "JPY", "rate": 15.800 },
                    { "validFor": "2026-01-02", "amount": 1, "currencyCode": "NZD", "rate": 12.200 }
                  ]
                }
                """);

            var rates = CnbExchangeRateClient.ParseDailyRates(document.RootElement);

            rates.Should().BeEquivalentTo(
                [
                    new CnbPublishedRate(new DateTime(2026, 1, 2), "PLN", 1, 5.500m),
                    new CnbPublishedRate(new DateTime(2026, 1, 2), "JPY", 100, 15.800m)
                ]);
        }

        [Fact]
        public void ConvertDailyRatesToPlnRates_ShouldStoreRepeatedWeekendResponseOnlyOnce()
        {
            var rates = new[]
            {
                new CnbPublishedRate(new DateTime(2026, 1, 2), "PLN", 1, 5.50m),
                new CnbPublishedRate(new DateTime(2026, 1, 2), "USD", 1, 22.00m),
                new CnbPublishedRate(new DateTime(2026, 1, 2), "PLN", 1, 5.50m),
                new CnbPublishedRate(new DateTime(2026, 1, 2), "USD", 1, 22.00m)
            };

            var tables = CnbExchangeRateClient.ConvertDailyRatesToPlnRates(
                rates,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 4));

            tables.Should().ContainSingle()
                .Which.EffectiveDate.Should().Be(new DateTime(2026, 1, 2));
        }
    }
}
