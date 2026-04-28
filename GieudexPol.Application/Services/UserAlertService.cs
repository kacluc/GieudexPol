using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class UserAlertService : IUserAlertService
    {
        private readonly IUserAlertRepository _userAlertRepository;

        public UserAlertService(IUserAlertRepository userAlertRepository)
        {
            _userAlertRepository = userAlertRepository;
        }

        public async Task<UserAlert> GetByIdAsync(int id)
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
    }
}