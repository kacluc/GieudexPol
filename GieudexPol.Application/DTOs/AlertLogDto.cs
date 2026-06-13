namespace GieudexPol.Application.DTOs
{
    public class AlertLogDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal? CurrentAmount { get; set; }
        public string? SourceSummary { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
}
