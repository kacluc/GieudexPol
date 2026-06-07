using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface INotificationService : IService<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
        Task MarkNotificationAsReadAsync(int notificationId);
    }
}
