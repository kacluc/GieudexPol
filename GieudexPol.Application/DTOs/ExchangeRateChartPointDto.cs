namespace GieudexPol.Application.DTOs
{
    public class ExchangeRateChartPointDto
    {
        public DateTime Date { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal? MidPrice { get; set; }
    }
}
