using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface IWhaleRankingRepository
    {
        Task<IEnumerable<WhaleRanking>> GetAllAsync();
        Task<WhaleRanking?> GetByIdAsync(int id);
        Task<WhaleRanking?> GetByUserIdAsync(int userId);
        Task AddAsync(WhaleRanking entity);
        Task UpdateAsync(WhaleRanking entity);
        Task DeleteAsync(WhaleRanking entity);
        Task<IEnumerable<WhaleRanking>> GetTopWhalesAsync(int topN);
        Task RefreshRankingAsync();
    }
}