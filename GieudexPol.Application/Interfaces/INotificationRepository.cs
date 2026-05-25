
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IEnumerable<Notification>> GetAllAsync();
        Task AddAsync(Notification entity);
        Task UpdateAsync(Notification entity);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
    }
}
