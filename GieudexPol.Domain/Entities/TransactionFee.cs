
namespace GieudexPol.Domain.Entities
{
    public class TransactionFee
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal FeePercentage { get; set; }
        public decimal FlatFee { get; set; }
        public bool IsActive { get; set; }
    }
}
