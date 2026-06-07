using FluentAssertions;
using GieudexPol.Infrastructure.ExternalServices.BankOfCanada;

namespace GieudexPol.Tests;

public class BankOfCanadaExchangeRateClientTests
{
    [Fact]
    public void CalculateCrossRateToPln_ShouldConvertUsdCadUsingPlnCad()
    {
        var result =
            BankOfCanadaExchangeRateClient.CalculateCrossRateToPln(1.3700m, 0.3790m);

        result.Should().Be(Math.Round(1.3700m / 0.3790m, 4));
    }

    [Fact]
    public void CalculateCadRateToPln_ShouldInvertPlnCad()
    {
        var result = BankOfCanadaExchangeRateClient.CalculateCadRateToPln(0.3790m);

        result.Should().Be(Math.Round(1m / 0.3790m, 4));
    }

    [Fact]
    public void ParseObservations_ShouldSkipMissingSeriesWithoutDroppingTheDay()
    {
        const string json = """
            {
              "observations": [
                {
                  "d": "2026-06-05",
                  "FXUSDCAD": { "v": "1.3700" },
                  "FXEURCAD": { "v": "1.5600" },
                  "FXPLNCAD": { "v": "0.3790" }
                }
              ]
            }
            """;

        var tables = BankOfCanadaExchangeRateClient.ParseObservations(
            json,
            new DateTime(2026, 6, 5),
            new DateTime(2026, 6, 5));

        var table = tables.Should().ContainSingle().Which;
        table.Rates.Should().Contain(rate =>
            rate.CurrencyCode == "USD" &&
            rate.ReferenceRate == Math.Round(1.3700m / 0.3790m, 4));
        table.Rates.Should().Contain(rate =>
            rate.CurrencyCode == "CAD" &&
            rate.ReferenceRate == Math.Round(1m / 0.3790m, 4));
        table.Rates.Should().NotContain(rate => rate.CurrencyCode == "GBP");
    }

    [Fact]
    public void ParseObservations_ShouldSkipDayWithoutPlnCad()
    {
        const string json = """
            {
              "observations": [
                {
                  "d": "2026-06-05",
                  "FXUSDCAD": { "v": "1.3700" }
                }
              ]
            }
            """;

        var tables = BankOfCanadaExchangeRateClient.ParseObservations(
            json,
            new DateTime(2026, 6, 5),
            new DateTime(2026, 6, 5));

        tables.Should().BeEmpty();
    }
}
