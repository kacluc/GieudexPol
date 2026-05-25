
namespace GieudexPol.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Changes { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public User User { get; set; } = null!;
    }
}
