using System.Text.Json;
using FluentAssertions;
using GieudexPol.Infrastructure.ExternalServices.Riksbank;

namespace GieudexPol.Tests
{
    public class RiksbankExchangeRateClientTests
    {
        [Fact]
        public void ConvertObservationsToPlnRates_ShouldConvertUsdRateToPln()
        {
            var date = new DateTime(2026, 1, 2);
            var observations = new[]
            {
                new RiksbankObservation("SEKPLNPMI", date, 1m / 0.39m),
                new RiksbankObservation("SEKUSDPMI", date, 1m / 0.095m)
            };

            var tables = RiksbankExchangeRateClient.ConvertObservationsToPlnRates(
                observations,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var usdRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "USD");
            usdRate.BuyPrice.Should().Be(decimal.Round(0.39m / 0.095m, 6, MidpointRounding.AwayFromZero));
            usdRate.SellPrice.Should().Be(usdRate.BuyPrice);
        }

        [Fact]
        public void ConvertObservationsToPlnRates_ShouldUsePlnRateForSek()
        {
            var date = new DateTime(2026, 1, 2);
            var observations = new[]
            {
                new RiksbankObservation("SEKPLNPMI", date, 1m / 0.39m),
                new RiksbankObservation("SEKEURPMI", date, 1m / 0.09m)
            };

            var tables = RiksbankExchangeRateClient.ConvertObservationsToPlnRates(
                observations,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var sekRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "SEK");
            sekRate.BuyPrice.Should().Be(0.39m);
            sekRate.SellPrice.Should().Be(0.39m);
        }

        [Fact]
        public void ConvertObservationsToPlnRates_ShouldSkipDayWithoutPlnRate()
        {
            var date = new DateTime(2026, 1, 2);
            var observations = new[]
            {
                new RiksbankObservation("SEKUSDPMI", date, 1m / 0.095m)
            };

            var tables = RiksbankExchangeRateClient.ConvertObservationsToPlnRates(
                observations,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            tables.Should().BeEmpty();
        }

        [Fact]
        public void ParseGroupObservations_ShouldReadNestedResponse()
        {
            using var document = JsonDocument.Parse("""
                {
                  "items": [
                    { "seriesId": "SEKPLNPMI", "date": "2026-01-02", "value": 2.56 },
                    { "seriesId": "SEKUSDPMI", "date": "2026-01-02", "value": 9.37 }
                  ]
                }
                """);

            var observations = RiksbankExchangeRateClient.ParseGroupObservations(document.RootElement);

            observations.Should().BeEquivalentTo(
                [
                    new RiksbankObservation("SEKPLNPMI", new DateTime(2026, 1, 2), 2.56m),
                    new RiksbankObservation("SEKUSDPMI", new DateTime(2026, 1, 2), 9.37m)
                ]);
        }

        [Fact]
        public void ParseGroupObservations_ShouldUseParentSeriesIdForNestedObservations()
        {
            using var document = JsonDocument.Parse("""
                {
                  "seriesId": "SEKUSDPMI",
                  "observations": [
                    { "date": "2026-01-02", "value": 9.37 }
                  ]
                }
                """);

            var observations = RiksbankExchangeRateClient.ParseGroupObservations(document.RootElement);

            observations.Should().ContainSingle()
                .Which.Should().Be(new RiksbankObservation("SEKUSDPMI", new DateTime(2026, 1, 2), 9.37m));
        }
    }
}
