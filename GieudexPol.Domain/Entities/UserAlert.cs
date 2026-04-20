using System;

namespace GieudexPol.Domain.Entities
{
    public class UserAlert
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public decimal TargetPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}