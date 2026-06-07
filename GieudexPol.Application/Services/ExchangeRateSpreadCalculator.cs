namespace GieudexPol.Application.Services;

public static class ExchangeRateSpreadCalculator
{
    public static (decimal buyPrice, decimal sellPrice) CalculateSyntheticBidAsk(
        decimal referenceRate,
        decimal spreadPercent)
    {
        if (referenceRate <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(referenceRate),
                "Reference rate must be greater than zero.");
        }

        if (spreadPercent <= 0m || spreadPercent >= 2m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(spreadPercent),
                "Spread percent must be greater than zero and lower than 2.");
        }

        var halfSpread = spreadPercent / 2m;
        var buyPrice = referenceRate * (1m - halfSpread);
        var sellPrice = referenceRate * (1m + halfSpread);
        var roundedBuyPrice = decimal.Round(buyPrice, 4, MidpointRounding.AwayFromZero);
        var roundedSellPrice = decimal.Round(sellPrice, 4, MidpointRounding.AwayFromZero);

        if (roundedBuyPrice < roundedSellPrice)
        {
            return (roundedBuyPrice, roundedSellPrice);
        }

        const decimal minimumPriceStep = 0.0001m;
        var roundedReferenceRate = decimal.Round(referenceRate, 4, MidpointRounding.AwayFromZero);

        return (
            Math.Max(minimumPriceStep, roundedReferenceRate - minimumPriceStep),
            roundedReferenceRate + minimumPriceStep);
    }
}
