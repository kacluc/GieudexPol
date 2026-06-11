using System.ComponentModel.DataAnnotations;

namespace GieudexPol.Application.DTOs
{
    public class AdminTestRateSourceDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class AdminTestExchangeRateDto
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal MidPrice { get; set; }
        public string RateSourceCode { get; set; } = string.Empty;
        public string RateSourceName { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
    }

    public class CreateTestExchangeRateDto
    {
        public string? RateSourceCode { get; set; }

        public int? CurrencyId { get; set; }

        public string? CurrencyCode { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal BuyPrice { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal SellPrice { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal? MidPrice { get; set; }
    }

    public class UpdateTestExchangeRateDto
    {
        [Required]
        public DateTime EffectiveDate { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal BuyPrice { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal SellPrice { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal? MidPrice { get; set; }
    }
}
