using GieudexPol.Domain.Entities;
using System;

namespace GieudexPol.Application.DTOs
{
    public class UserAlertCreateDto
    {
        public int CurrencyId { get; set; }
        public AlertType AlertType { get; set; }
        public AlertPriceSide PriceSide { get; set; }
        public ThresholdDirection? ThresholdDirection { get; set; }
        public int? RateSourceId { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? PercentageChange { get; set; }
        public int? TimeFrameHours { get; set; }
    }

    public class UserAlertDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencySymbol { get; set; } = string.Empty;
        public AlertType AlertType { get; set; }
        public AlertPriceSide PriceSide { get; set; }
        public ThresholdDirection? ThresholdDirection { get; set; }
        public int? RateSourceId { get; set; }
        public string? RateSourceCode { get; set; }
        public string? RateSourceName { get; set; }
        public bool AppliesToAllRateSources => !RateSourceId.HasValue;
        public decimal? ThresholdValue { get; set; }
        public decimal? PercentageChange { get; set; }
        public int? TimeFrameHours { get; set; }
        public AlertStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TriggeredDate { get; set; }
        public IReadOnlyList<AlertLogDto> Logs { get; set; } = [];
    }

    public class UserAlertUpdateDto
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public AlertType AlertType { get; set; }
        public AlertPriceSide PriceSide { get; set; }
        public ThresholdDirection? ThresholdDirection { get; set; }
        public int? RateSourceId { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? PercentageChange { get; set; }
        public int? TimeFrameHours { get; set; }
        public AlertStatus Status { get; set; }
    }

    public class AlertRateSourceDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
