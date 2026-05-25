namespace GieudexPol.Application.DTOs;

public class CurrencyExchangeSimulationResponseDto
{
    public decimal OriginalAmount { get; set; }

    public decimal ExchangedAmount { get; set; }

    public string SourceCurrency { get; set; } = string.Empty;

    public string TargetCurrency { get; set; } = string.Empty;

    public decimal ExchangeRate { get; set; }

    public decimal FeeAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public DateTime SimulationDate { get; set; }
}
