using System;

namespace GieudexPol.Domain.Entities
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; } = null!;
        public int RateSourceId { get; set; }
        public RateSource RateSource { get; set; } = null!;
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal? MidPrice { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime FetchedAt { get; set; }
    }
}
