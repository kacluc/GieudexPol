using System.Text.Json;
using FluentAssertions;
using GieudexPol.Infrastructure.ExternalServices.Norges;

namespace GieudexPol.Tests
{
    public class NorgesExchangeRateClientTests
    {
        [Fact]
        public void ConvertObservationsToPlnRates_ShouldConvertUsdRateToPln()
        {
            var observations = new[]
            {
                new NorgesObservation("PLN", new DateTime(2026, 1, 2), 1, 2.60m),
                new NorgesObservation("USD", new DateTime(2026, 1, 2), 1, 10.50m)
            };

            var tables = NorgesExchangeRateClient.ConvertObservationsToPlnRates(
                observations,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var usdRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "USD");
            var expected = decimal.Round(10.50m / 2.60m, 6, MidpointRounding.AwayFromZero);
            usdRate.BuyPrice.Should().Be(expected);
            usdRate.SellPrice.Should().Be(expected);
        }

        [Fact]
        public void ConvertObservationsToPlnRates_ShouldCreateNokRateFromPlnRate()
        {
            var observations = new[]
            {
                new NorgesObservation("PLN", new DateTime(2026, 1, 2), 1, 2.60m)
            };

            var tables = NorgesExchangeRateClient.ConvertObservationsToPlnRates(
                observations,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var nokRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "NOK");
            nokRate.BuyPrice.Should().Be(decimal.Round(1m / 2.60m, 6, MidpointRounding.AwayFromZero));
            nokRate.SellPrice.Should().Be(nokRate.BuyPrice);
        }

        [Fact]
        public void ConvertObservationsToPlnRates_ShouldRespectHundredCurrencyUnit()
        {
            var observations = new[]
            {
                new NorgesObservation("PLN", new DateTime(2026, 1, 2), 1, 2.60m),
                new NorgesObservation("JPY", new DateTime(2026, 1, 2), 100, 6.80m)
            };

            var tables = NorgesExchangeRateClient.ConvertObservationsToPlnRates(
                observations,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            var jpyRate = tables.Single().Rates.Single(rate => rate.CurrencyCode == "JPY");
            var expected = decimal.Round((6.80m / 100m) / 2.60m, 6, MidpointRounding.AwayFromZero);
            jpyRate.BuyPrice.Should().Be(expected);
            jpyRate.SellPrice.Should().Be(expected);
        }

        [Fact]
        public void ConvertObservationsToPlnRates_ShouldFailClearlyWhenPublishedDayHasNoPlnRate()
        {
            var observations = new[]
            {
                new NorgesObservation("USD", new DateTime(2026, 1, 2), 1, 10.50m)
            };

            var action = () => NorgesExchangeRateClient.ConvertObservationsToPlnRates(
                observations,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 3));

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*NOK/PLN*normalized to PLN*");
        }

        [Fact]
        public void ParseDailyObservations_ShouldReadSdmxJsonUnitMultiplierAndFilterUnsupportedCurrency()
        {
            using var document = JsonDocument.Parse("""
                {
                  "data": {
                    "dataSets": [{
                      "series": {
                        "0:0:0:0": { "attributes": [0, 0, 0, 0], "observations": { "0": ["6.8000"] } },
                        "0:1:0:0": { "attributes": [0, 0, 1, 0], "observations": { "0": ["2.6000"] } },
                        "0:2:0:0": { "attributes": [0, 0, 1, 0], "observations": { "0": ["1.2000"] } }
                      }
                    }],
                    "structure": {
                      "dimensions": {
                        "series": [
                          { "id": "FREQ", "values": [{ "id": "B" }] },
                          { "id": "BASE_CUR", "values": [{ "id": "JPY" }, { "id": "PLN" }, { "id": "NZD" }] },
                          { "id": "QUOTE_CUR", "values": [{ "id": "NOK" }] },
                          { "id": "TENOR", "values": [{ "id": "SP" }] }
                        ],
                        "observation": [
                          {
                            "id": "TIME_PERIOD",
                            "values": [{ "id": "2026-01-02" }]
                          }
                        ]
                      },
                      "attributes": {
                        "series": [
                          { "id": "DECIMALS", "values": [{ "id": "4" }] },
                          { "id": "CALCULATED", "values": [{ "id": "false" }] },
                          { "id": "UNIT_MULT", "values": [{ "id": "2" }, { "id": "0" }] },
                          { "id": "COLLECTION", "values": [{ "id": "C" }] }
                        ]
                      }
                    }
                  }
                }
                """);

            var observations = NorgesExchangeRateClient.ParseDailyObservations(document.RootElement);

            observations.Should().BeEquivalentTo(
                [
                    new NorgesObservation("JPY", new DateTime(2026, 1, 2), 100, 6.8000m),
                    new NorgesObservation("PLN", new DateTime(2026, 1, 2), 1, 2.6000m)
                ]);
        }
    }
}
