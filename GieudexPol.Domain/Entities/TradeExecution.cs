namespace GieudexPol.Domain.Entities
{
    public class TradeExecution
    {
        public int Id { get; set; }
        public int BuyOrderId { get; set; }
        public Order BuyOrder { get; set; } = null!;
        public int SellOrderId { get; set; }
        public Order SellOrder { get; set; } = null!;
        public int TradingPairId { get; set; }
        public TradingPair TradingPair { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExecutedAt { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
