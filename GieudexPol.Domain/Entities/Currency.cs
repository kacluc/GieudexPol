using System.Collections.Generic;

namespace GieudexPol.Domain.Entities
{
    public class Currency
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Wallet> Wallets { get; set; }
        public ICollection<ExchangeRate> ExchangeRates { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<UserAlert> UserAlerts { get; set; }
    }
}