namespace GieudexPol.Domain.Entities;

public class CurrencyExchangeSimulation
{
    public decimal Amount { get; set; } //kwota wejsciowa

    public string SourceCurrency { get; set; } = string.Empty; //waluta zrodlowa

    public string TargetCurrency { get; set; } = string.Empty; //waluta docelowa

    public decimal ExchangeRate { get; set; } //kurs wymiany

    public decimal FeePercent { get; set; } //prowizja procentowa

    public decimal FeeAmount { get; set; } //prowizja kwotowa

    public decimal FinalAmount { get; set; } //koncowa kwota po wymianie

    public DateTime SimulationDate { get; set; } = DateTime.UtcNow; //data wykonania symulacji
}