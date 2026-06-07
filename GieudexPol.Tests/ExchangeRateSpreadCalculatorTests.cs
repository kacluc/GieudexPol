using FluentAssertions;
using GieudexPol.Application.Services;

namespace GieudexPol.Tests;

public class ExchangeRateSpreadCalculatorTests
{
    [Fact]
    public void CalculateSyntheticBidAsk_ShouldSplitTwoPercentSpreadAroundReferenceRate()
    {
        var (buyPrice, sellPrice) =
            ExchangeRateSpreadCalculator.CalculateSyntheticBidAsk(5.00m, 0.02m);

        buyPrice.Should().Be(4.95m);
        sellPrice.Should().Be(5.05m);
    }

    [Fact]
    public void CalculateSyntheticBidAsk_ShouldRoundPricesToFourDecimalPlaces()
    {
        var (buyPrice, sellPrice) =
            ExchangeRateSpreadCalculator.CalculateSyntheticBidAsk(4.3210m, 0.02m);

        buyPrice.Should().Be(Math.Round(4.3210m * 0.99m, 4));
        sellPrice.Should().Be(Math.Round(4.3210m * 1.01m, 4));
    }

    [Fact]
    public void CalculateSyntheticBidAsk_ShouldKeepPricesDistinctForLowReferenceRate()
    {
        var (buyPrice, sellPrice) =
            ExchangeRateSpreadCalculator.CalculateSyntheticBidAsk(0.0024m, 0.02m);

        buyPrice.Should().Be(0.0023m);
        sellPrice.Should().Be(0.0025m);
        buyPrice.Should().BeLessThan(sellPrice);
    }
}
