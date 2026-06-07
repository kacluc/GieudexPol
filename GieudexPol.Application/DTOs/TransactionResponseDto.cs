namespace GieudexPol.Application.DTOs
{
    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal AppliedFee { get; set; }
        public Guid? TransactionFeeId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
