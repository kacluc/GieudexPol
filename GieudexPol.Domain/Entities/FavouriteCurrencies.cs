namespace GieudexPol.Domain.Entities
{
    public class FavoriteCurrency
    {
        public int Id { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
