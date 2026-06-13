using System;

namespace GieudexPol.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public int ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; } = null!;
        public string Status { get; set; } = string.Empty; // Pending, Completed, Failed
        public string TransactionType { get; set; } = string.Empty; // Transfer, Buy, Sell
        public decimal AppliedFee { get; set; } // The actual fee applied for this transaction
        public Guid? TransactionFeeId { get; set; }
        public TransactionFee? TransactionFee { get; set; }
        public int? TradeExecutionId { get; set; }
        public TradeExecution? TradeExecution { get; set; }
        public int? ExchangeExecutionId { get; set; }
        public ExchangeExecution? ExchangeExecution { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
