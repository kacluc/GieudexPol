using System;

namespace GieudexPol.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int ReceiverId { get; set; }
        public User Receiver { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public string Status { get; set; } // Pending, Completed, Failed
        public string TransactionType { get; set; } // Transfer, Buy, Sell
        public decimal AppliedFee { get; set; } // The actual fee applied for this transaction
        public Guid? TransactionFeeId { get; set; }
        public TransactionFee TransactionFee { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
