using System;

namespace GieudexPol.Application.DTOs
{
    public class WhaleRankingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal TotalPortfolioValue { get; set; }
        public int Rank { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}