using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class WhaleRankingService : IWhaleRankingService
    {
        private readonly IWhaleRankingRepository _whaleRankingRepository;

        public WhaleRankingService(IWhaleRankingRepository whaleRankingRepository)
        {
            _whaleRankingRepository = whaleRankingRepository;
        }

        public async Task<IEnumerable<WhaleRanking>> GetAllAsync()
        {
            return await _whaleRankingRepository.GetAllAsync();
        }

        public async Task<WhaleRanking?> GetByIdAsync(int id)
        {
            return await _whaleRankingRepository.GetByIdAsync(id);
        }

        public async Task<WhaleRanking?> GetByUserIdAsync(int userId)
        {
            return await _whaleRankingRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<WhaleRanking>> GetTopWhalesAsync(int topN)
        {
            return await _whaleRankingRepository.GetTopWhalesAsync(topN);
        }

        public async Task RefreshRankingAsync()
        {
            await _whaleRankingRepository.RefreshRankingAsync();
        }
    }
}