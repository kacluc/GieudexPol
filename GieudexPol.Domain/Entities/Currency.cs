using System.Collections.Generic;

namespace GieudexPol.Domain.Entities
{
    public class Currency
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
        public ICollection<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<UserAlert> UserAlerts { get; set; } = new List<UserAlert>();
    }
}
