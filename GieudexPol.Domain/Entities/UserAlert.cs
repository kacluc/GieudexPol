using System;

namespace GieudexPol.Domain.Entities
{
    public enum AlertType
    {
        PriceDrop,
        PriceIncrease,
        Threshold
    }

    public enum AlertPriceSide
    {
        UserBuysCurrency,
        UserSellsCurrency,
        MidPrice
    }

    public enum ThresholdDirection
    {
        AboveOrEqual,
        BelowOrEqual
    }

    public class UserAlert
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public AlertType AlertType { get; set; }
        public AlertPriceSide PriceSide { get; set; }
        public ThresholdDirection? ThresholdDirection { get; set; }
        public int? RateSourceId { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? PercentageChange { get; set; }
        public int? TimeFrameHours { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TriggeredDate { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedDate { get; set; }

        public User User { get; set; } = null!;
        public Currency Currency { get; set; } = null!;
        public RateSource? RateSource { get; set; }
        public ICollection<UserAlertEvaluationState> EvaluationStates { get; set; } =
            new List<UserAlertEvaluationState>();
    }
}
