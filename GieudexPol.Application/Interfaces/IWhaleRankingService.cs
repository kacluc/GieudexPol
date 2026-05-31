using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface IWhaleRankingService
    {
        Task<IEnumerable<WhaleRanking>> GetAllAsync();
        Task<WhaleRanking?> GetByIdAsync(int id);
        Task<WhaleRanking?> GetByUserIdAsync(int userId);
        Task<IEnumerable<WhaleRanking>> GetTopWhalesAsync(int topN);
        Task RefreshRankingAsync();
    }
}