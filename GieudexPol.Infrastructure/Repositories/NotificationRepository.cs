
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        // In a real application, this would interact with a database context.
        // For now, we\'ll use a simple in-memory collection.
        private readonly List<Notification> _notifications = new List<Notification>();

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_notifications.FirstOrDefault(n => n.Id == id));
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await Task.FromResult(_notifications);
        }

        public async Task AddAsync(Notification entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            _notifications.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Notification entity)
        {
            var existingNotification = _notifications.FirstOrDefault(n => n.Id == entity.Id);
            if (existingNotification != null)
            {
                existingNotification.Message = entity.Message;
                existingNotification.IsRead = entity.IsRead;
                existingNotification.UserId = entity.UserId;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            _notifications.RemoveAll(n => n.Id == id);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId)
        {
            return await Task.FromResult(_notifications.Where(n => n.UserId == userId).ToList());
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
            }
            await Task.CompletedTask;
        }
    }
}
