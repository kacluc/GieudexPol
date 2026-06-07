using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class UserAlertService : IUserAlertService
    {
        private readonly IUserAlertRepository _userAlertRepository;
        private readonly INotificationService _notificationService;

        public UserAlertService(IUserAlertRepository userAlertRepository, INotificationService notificationService)
        {
            _userAlertRepository = userAlertRepository;
            _notificationService = notificationService;
        }

        public async Task<UserAlert?> GetByIdAsync(int id)
        {
            return await _userAlertRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<UserAlert>> GetAllAsync()
        {
            return await _userAlertRepository.GetAllAsync();
        }

        public async Task AddAsync(UserAlert entity)
        {
            await _userAlertRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(UserAlert entity)
        {
            await _userAlertRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(UserAlert entity)
        {
            await _userAlertRepository.DeleteAsync(entity);
        }

        public async Task<IEnumerable<UserAlert>> GetUserAlertsByUserIdAsync(int userId)
        {
            return await _userAlertRepository.GetUserAlertsByUserIdAsync(userId);
        }

        public async Task CreateUserAlertAsync(UserAlert userAlert)
        {
            userAlert.CreatedDate = System.DateTime.UtcNow;
            userAlert.IsActive = true;
            await _userAlertRepository.AddAsync(userAlert);
        }

        public async Task UpdateUserAlertAsync(UserAlert userAlert)
        {
            await _userAlertRepository.UpdateAsync(userAlert);
        }

        public async Task DeleteUserAlertAsync(int userAlertId)
        {
            var userAlert = await _userAlertRepository.GetByIdAsync(userAlertId);
            if (userAlert != null)
            {
                await _userAlertRepository.DeleteAsync(userAlert);
            }
        }

        public async Task TriggerAlertAsync(int userAlertId, string message)
        {
            var userAlert = await _userAlertRepository.GetByIdAsync(userAlertId);
            if (userAlert != null)
            {
                userAlert.TriggeredDate = System.DateTime.UtcNow;
                userAlert.IsActive = false; // Deactivate alert after triggering
                await _userAlertRepository.UpdateAsync(userAlert);

                var notification = new Notification
                {
                    UserId = userAlert.UserId,
                    Message = message,
                    CreatedDate = System.DateTime.UtcNow,
                    IsRead = false
                };
                await _notificationService.AddAsync(notification);
            }
        }
    }
}
