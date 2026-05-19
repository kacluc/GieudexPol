using System;
using System.Collections.Generic;

namespace GieudexPol.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public Guid AuthId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Admin/User
        public ICollection<Wallet> Wallets { get; set; }
        public ICollection<UserAlert> UserAlerts { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Transaction> SentTransactions { get; set; }
        public ICollection<Transaction> ReceivedTransactions { get; set; }
    }
}
