using System;
using System.Collections.Generic;

namespace GieudexPol.Domain.Entities
{
    public enum AccountType
    {
        RegularUser,
        AdminUser,
        SuperAdminUser,
        RateSourceSystem,
        PlatformTreasury
    }

    public class User
    {
        public int Id { get; set; }
        public Guid AuthId { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin/User
        public AccountType AccountType { get; set; } = AccountType.RegularUser;
        public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
        public ICollection<UserAlert> UserAlerts { get; set; } = new List<UserAlert>();
        public ICollection<UserTradingAlert> UserTradingAlerts { get; set; } =
            new List<UserTradingAlert>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ExchangeExecution> ExchangeExecutions { get; set; } =
            new List<ExchangeExecution>();
    }
}
