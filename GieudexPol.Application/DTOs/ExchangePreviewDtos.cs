namespace GieudexPol.Application.DTOs
{
    public class ExchangePreviewRequestDto
    {
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public decimal Amount { get; set; }
    }

    public class ExchangePreviewResultDto
    {
        public string FromCurrencyCode { get; set; } = string.Empty;
        public string ToCurrencyCode { get; set; } = string.Empty;
        public decimal InputAmount { get; set; }
        public decimal EstimatedOutputAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal FeeAmount { get; set; }
        public string FeeCurrencyCode { get; set; } = string.Empty;
        public decimal TotalDebitAmount { get; set; }
        public DateTime RateDate { get; set; }
        public string RateSourceCode { get; set; } = string.Empty;
        public string RateSourceName { get; set; } = string.Empty;
        public bool HasSufficientFunds { get; set; }
        public bool IsPreview { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}
