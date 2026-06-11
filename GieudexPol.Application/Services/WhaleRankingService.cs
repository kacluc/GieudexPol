using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<WhaleRankingDto>> GetAllAsync()
        {
            var rankings = await _whaleRankingRepository.GetAllAsync();
            return rankings.Select(MapToDto);
        }

        public async Task<WhaleRankingDto?> GetByIdAsync(int id)
        {
            var ranking = await _whaleRankingRepository.GetByIdAsync(id);
            return ranking == null ? null : MapToDto(ranking);
        }

        public async Task<WhaleRankingDto?> GetByUserIdAsync(int userId)
        {
            var ranking = await _whaleRankingRepository.GetByUserIdAsync(userId);
            return ranking == null ? null : MapToDto(ranking);
        }

        public async Task<IEnumerable<WhaleRankingDto>> GetTopWhalesAsync(int topN)
        {
            var rankings = await _whaleRankingRepository.GetTopWhalesAsync(topN);
            return rankings.Select(MapToDto);
        }

        public async Task RefreshRankingAsync()
        {
            await _whaleRankingRepository.RefreshRankingAsync();
        }

        private static WhaleRankingDto MapToDto(WhaleRanking ranking)
        {
            return new WhaleRankingDto
            {
                Id = ranking.Id,
                UserId = ranking.UserId,
                Username = string.IsNullOrWhiteSpace(ranking.User.DisplayName)
                    ? GetEmailLocalPart(ranking.User.Username)
                    : ranking.User.DisplayName,
                TotalPortfolioValue = ranking.TotalPortfolioValue,
                Rank = ranking.Rank,
                LastUpdated = ranking.LastUpdated
            };
        }

        private static string GetEmailLocalPart(string username)
        {
            var separatorIndex = username.IndexOf('@');
            return separatorIndex > 0 ? username[..separatorIndex] : username;
        }
    }
}
