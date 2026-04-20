using System;

namespace GieudexPol.Domain.Entities
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public decimal BuyPrice { get; set; } // decimal(18,4)
        public decimal SellPrice { get; set; } // decimal(18,4)
        public DateTime Timestamp { get; set; }
    }
}