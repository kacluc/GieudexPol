namespace GieudexPol.Domain.Entities
{
    public class ExchangeExecution
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int RateSourceId { get; set; }
        public RateSource RateSource { get; set; } = null!;
        public int FromCurrencyId { get; set; }
        public Currency FromCurrency { get; set; } = null!;
        public int ToCurrencyId { get; set; }
        public Currency ToCurrency { get; set; } = null!;
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal FeeAmount { get; set; }
        public int FeeCurrencyId { get; set; }
        public Currency FeeCurrency { get; set; } = null!;
        public DateTime ExecutedAt { get; set; }
        public ICollection<Transaction> Transactions { get; set; } =
            new List<Transaction>();
    }
}
