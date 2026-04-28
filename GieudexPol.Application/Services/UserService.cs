using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task AddAsync(User entity)
        {
            await _userRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(User entity)
        {
            await _userRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(User entity)
        {
            await _userRepository.DeleteAsync(entity);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }
    }
}