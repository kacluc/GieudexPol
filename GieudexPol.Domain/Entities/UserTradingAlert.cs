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
        public AlertStatus Status { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsActive
        {
            get => Status == AlertStatus.Active;
            set => Status = value ? AlertStatus.Active : AlertStatus.Inactive;
        }
        public DateTime CreatedDate { get; set; }
        public DateTime? TriggeredDate { get; set; }
        public ICollection<AlertLog> Logs { get; set; } = new List<AlertLog>();
    }
}
