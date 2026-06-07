using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Notification> GetByIdAsync(int id)
        {
            return await _notificationRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _notificationRepository.GetAllAsync();
        }

        public async Task AddAsync(Notification entity)
        {
            await _notificationRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(Notification entity)
        {
            await _notificationRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Notification entity)
        {
            await _notificationRepository.DeleteAsync(entity);
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
        {
            return await _notificationRepository.GetUserNotificationsAsync(userId);
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            await _notificationRepository.MarkNotificationAsReadAsync(notificationId);
        }
    }
}
