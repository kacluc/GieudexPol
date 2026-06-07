namespace GieudexPol.Application.DTOs
{
    public class ExchangeRateTableRowDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal? MidPrice { get; set; }
    }
}
