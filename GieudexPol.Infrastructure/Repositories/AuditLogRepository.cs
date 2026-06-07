
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        // In a real application, this would interact with a database context.
        // For now, we'll use a simple in-memory collection.
        private readonly List<AuditLog> _auditLogs = new List<AuditLog>();

        public async Task<AuditLog?> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_auditLogs.FirstOrDefault(al => al.Id == id));
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await Task.FromResult(_auditLogs);
        }

        public async Task AddAsync(AuditLog entity)
        {
            entity.Id = Guid.NewGuid();
            entity.Timestamp = DateTime.UtcNow;
            _auditLogs.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(AuditLog entity)
        {
            var existingLog = _auditLogs.FirstOrDefault(al => al.Id == entity.Id);
            if (existingLog != null)
            {
                existingLog.Action = entity.Action;
                existingLog.EntityName = entity.EntityName;
                existingLog.EntityId = entity.EntityId;
                existingLog.Changes = entity.Changes;
                existingLog.UserId = entity.UserId;
                existingLog.Timestamp = DateTime.UtcNow; // Update timestamp on modification
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            _auditLogs.RemoveAll(al => al.Id == id);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByUserIdAsync(Guid userId)
        {
            return await Task.FromResult(_auditLogs.Where(al => al.UserId == userId).ToList());
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByEntityIdAsync(Guid entityId)
        {
            return await Task.FromResult(_auditLogs.Where(al => al.EntityId == entityId).ToList());
        }
    }
}
