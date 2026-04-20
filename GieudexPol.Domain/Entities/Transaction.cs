using System;

namespace GieudexPol.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public string TransactionType { get; set; } // Buy/Sell
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Commission { get; set; }
        public DateTime Timestamp { get; set; }
    }
}