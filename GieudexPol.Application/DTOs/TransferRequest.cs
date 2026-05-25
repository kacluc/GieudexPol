namespace GieudexPol.Application.DTOs
{
    public class TransferRequest
    {
        public int SenderId { get; set; }
        public string ReceiverUsername { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
    }
}
