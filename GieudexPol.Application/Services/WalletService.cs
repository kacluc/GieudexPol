using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;

        public WalletService(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<Wallet> GetByIdAsync(int id)
        {
            return await _walletRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Wallet>> GetAllAsync()
        {
            return await _walletRepository.GetAllAsync();
        }

        public async Task AddAsync(Wallet entity)
        {
            await _walletRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(Wallet entity)
        {
            await _walletRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Wallet entity)
        {
            await _walletRepository.DeleteAsync(entity);
        }

        public async Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId)
        {
            return await _walletRepository.GetUserWalletsAsync(userId);
        }
    }
}