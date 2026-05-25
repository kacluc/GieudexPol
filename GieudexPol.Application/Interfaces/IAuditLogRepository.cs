
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<AuditLog?> GetByIdAsync(Guid id);
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task AddAsync(AuditLog entity);
        Task UpdateAsync(AuditLog entity);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<AuditLog>> GetLogsByUserIdAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetLogsByEntityIdAsync(Guid entityId);
    }
}
