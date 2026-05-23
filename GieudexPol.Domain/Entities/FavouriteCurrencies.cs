namespace GieudexPol.Domain.Entities
{
    public class FavoriteCurrency
    {
        public int Id { get; set; }

        public string CurrencyCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}