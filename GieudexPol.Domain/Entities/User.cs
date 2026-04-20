using System;
using System.Collections.Generic;

namespace GieudexPol.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Admin/User
        public ICollection<Wallet> Wallets { get; set; }
        public ICollection<UserAlert> UserAlerts { get; set; }
    }
}