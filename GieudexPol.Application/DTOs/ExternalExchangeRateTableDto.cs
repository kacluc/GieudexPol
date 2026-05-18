using System.Text.Json.Serialization;

namespace GieudexPol.Application.DTOs
{
    public class ExternalExchangeRateTableDto
    {
        [JsonPropertyName("table")]
        public string Table { get; set; } = string.Empty;

        [JsonPropertyName("no")]
        public string Number { get; set; } = string.Empty;

        [JsonPropertyName("effectiveDate")]
        public DateTime EffectiveDate { get; set; }

        [JsonPropertyName("rates")]
        public List<ExternalExchangeRateItemDto> Rates { get; set; } = new();
    }
}
