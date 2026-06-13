namespace GieudexPol.Domain.Entities
{
    public enum AlertStatus
    {
        Active,
        Fulfilled,
        Inactive
    }

    public class AlertLog
    {
        public int Id { get; set; }
        public int? UserAlertId { get; set; }
        public UserAlert? UserAlert { get; set; }
        public int? UserTradingAlertId { get; set; }
        public UserTradingAlert? UserTradingAlert { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal? CurrentAmount { get; set; }
        public string? SourceSummary { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
}
