namespace GieudexPol.Application.Settings;

public class ExchangeRateSettings
{
    public const string SectionName = "ExchangeRateSettings";

    public decimal SyntheticSpreadPercent { get; set; } = 0.02m;
}
