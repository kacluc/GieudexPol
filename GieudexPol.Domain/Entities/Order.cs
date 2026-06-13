namespace GieudexPol.Domain.Entities
{
    public enum OrderSide
    {
        Buy,
        Sell
    }

    public enum OrderType
    {
        Limit
    }

    public enum OrderStatus
    {
        Open,
        PartiallyFilled,
        Filled,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int TradingPairId { get; set; }
        public TradingPair TradingPair { get; set; } = null!;
        public OrderSide Side { get; set; }
        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ExecutedQuoteAmount { get; set; }
        public decimal FeePaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public ICollection<TradeExecution> BuyExecutions { get; set; } = new List<TradeExecution>();
        public ICollection<TradeExecution> SellExecutions { get; set; } = new List<TradeExecution>();
    }
}
