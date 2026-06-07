using GieudexPol.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface IWhaleRankingService
    {
        Task<IEnumerable<WhaleRankingDto>> GetAllAsync();
        Task<WhaleRankingDto?> GetByIdAsync(int id);
        Task<WhaleRankingDto?> GetByUserIdAsync(int userId);
        Task<IEnumerable<WhaleRankingDto>> GetTopWhalesAsync(int topN);
        Task RefreshRankingAsync();
    }
}
