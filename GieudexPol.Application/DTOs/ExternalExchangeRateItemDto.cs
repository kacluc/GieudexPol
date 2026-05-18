using System.Text.Json.Serialization;

namespace GieudexPol.Application.DTOs
{
    public class ExternalExchangeRateItemDto
    {
        [JsonPropertyName("currency")]
        public string CurrencyName { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string CurrencyCode { get; set; } = string.Empty;

        [JsonPropertyName("bid")]
        public decimal BuyPrice { get; set; }

        [JsonPropertyName("ask")]
        public decimal SellPrice { get; set; }
    }
}
