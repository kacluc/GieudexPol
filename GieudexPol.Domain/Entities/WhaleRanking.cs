using System;

namespace GieudexPol.Domain.Entities
{
    public class WhaleRanking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public decimal TotalPortfolioValue { get; set; }
        public int Rank { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}