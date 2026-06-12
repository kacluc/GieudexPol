namespace GieudexPol.Application.DTOs
{
    public class AlertEvaluationRequest
    {
        public int? AlertId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? RateSourceCode { get; set; }
    }

    public class AlertEvaluationResult
    {
        public int EvaluatedAlertsCount { get; set; }
        public int TriggeredAlertsCount { get; set; }
        public int NotificationsCreatedCount { get; set; }
        public List<string> Details { get; set; } = new();
    }
}
