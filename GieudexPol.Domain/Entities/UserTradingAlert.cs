namespace GieudexPol.Domain.Entities
{
    public enum TradingAlertEvent
    {
        BuyOrder,
        SellOrder,
        TradeExecution
    }

    public class UserTradingAlert
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int TradingPairId { get; set; }
        public TradingPair TradingPair { get; set; } = null!;
        public TradingAlertEvent EventType { get; set; }
        public ThresholdDirection Direction { get; set; }
        public decimal TargetPrice { get; set; }
        public decimal? MinimumAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TriggeredDate { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
    }
}
