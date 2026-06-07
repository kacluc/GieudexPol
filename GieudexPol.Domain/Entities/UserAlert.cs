using System;

namespace GieudexPol.Domain.Entities
{
    public enum AlertType
    {
        PriceDrop,
        PriceIncrease,
        Threshold,
        Volume
    }

    public class UserAlert
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public AlertType AlertType { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? PercentageChange { get; set; }
        public int? TimeFrameHours { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TriggeredDate { get; set; }

        public User User { get; set; } = null!;
        public Currency Currency { get; set; } = null!;
    }
}
