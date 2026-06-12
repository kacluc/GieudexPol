namespace GieudexPol.Domain.Entities
{
    public class TradingPair
    {
        public int Id { get; set; }
        public int BaseCurrencyId { get; set; }
        public Currency BaseCurrency { get; set; } = null!;
        public int QuoteCurrencyId { get; set; }
        public Currency QuoteCurrency { get; set; } = null!;
        public bool IsActive { get; set; }
        public decimal TickSize { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<TradeExecution> TradeExecutions { get; set; } = new List<TradeExecution>();
        public ICollection<UserTradingAlert> UserTradingAlerts { get; set; } =
            new List<UserTradingAlert>();
    }
}
