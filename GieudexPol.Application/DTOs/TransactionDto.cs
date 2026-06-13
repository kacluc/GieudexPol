namespace GieudexPol.Application.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string ReceiverUsername { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencySymbol { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal AppliedFee { get; set; }
        public int? TradeExecutionId { get; set; }
        public string? TradingPair { get; set; }
        public decimal? ExecutionPrice { get; set; }
        public decimal? ExecutionAmount { get; set; }
        public int? ExchangeExecutionId { get; set; }
        public string? ExchangePair { get; set; }
        public string? RateSource { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? FeeCurrency { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
