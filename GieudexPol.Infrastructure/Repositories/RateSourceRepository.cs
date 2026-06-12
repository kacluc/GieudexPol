using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class RateSourceRepository : GenericRepository<RateSource>, IRateSourceRepository
    {
        public RateSourceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RateSource?> GetByCodeAsync(string code)
        {
            var normalizedCode = code.Trim().ToUpperInvariant();
            return await _dbSet.FirstOrDefaultAsync(rs => rs.Code == normalizedCode);
        }

        public async Task<IReadOnlyList<RateSource>> GetActiveAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(source => source.IsActive)
                .OrderBy(source => source.Code)
                .ToListAsync();
        }
    }
}
