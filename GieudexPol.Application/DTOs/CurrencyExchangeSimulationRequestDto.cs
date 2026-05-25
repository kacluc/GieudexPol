namespace GieudexPol.Application.DTOs;

public class CurrencyExchangeSimulationRequestDto
{
    public decimal Amount { get; set; }

    public string SourceCurrency { get; set; } = string.Empty;

    public string TargetCurrency { get; set; } = string.Empty;

    public decimal FeePercent { get; set; }
}