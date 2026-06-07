using GieudexPol.Domain.Entities;
using System;

namespace GieudexPol.Application.DTOs
{
    public class UserAlertCreateDto
    {
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public AlertType AlertType { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? PercentageChange { get; set; }
        public int? TimeFrameHours { get; set; }
    }

    public class UserAlertDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CurrencySymbol { get; set; } = string.Empty;
        public AlertType AlertType { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? PercentageChange { get; set; }
        public int? TimeFrameHours { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TriggeredDate { get; set; }
    }

    public class UserAlertUpdateDto
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public AlertType AlertType { get; set; }
        public decimal? ThresholdValue { get; set; }
        public decimal? PercentageChange { get; set; }
        public int? TimeFrameHours { get; set; }
        public bool IsActive { get; set; }
    }
}
