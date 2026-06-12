using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.DTOs
{
    public class UserTradingAlertCreateDto
    {
        public int TradingPairId { get; set; }
        public TradingAlertEvent EventType { get; set; }
        public ThresholdDirection Direction { get; set; }
        public decimal TargetPrice { get; set; }
        public decimal? MinimumAmount { get; set; }
    }

    public class UserTradingAlertUpdateDto : UserTradingAlertCreateDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserTradingAlertDto
    {
        public int Id { get; set; }
        public int TradingPairId { get; set; }
        public string Pair { get; set; } = string.Empty;
        public string BaseCurrency { get; set; } = string.Empty;
        public string QuoteCurrency { get; set; } = string.Empty;
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
