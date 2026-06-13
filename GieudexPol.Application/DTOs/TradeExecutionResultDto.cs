namespace GieudexPol.Application.DTOs
{
    public class TradeExecutionResultDto
    {
        public decimal AmountTo { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal FromRateToPln { get; set; }
        public decimal ToRateToPln { get; set; }
        public string SellRateSource { get; set; } = string.Empty;
        public string BuyRateSource { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public string RateSource { get; set; } = string.Empty;
        public decimal AppliedRate { get; set; }
        public decimal FeeAmount { get; set; }
        public string FeeCurrency { get; set; } = string.Empty;
        public int? ExchangeExecutionId { get; set; }
    }
}
