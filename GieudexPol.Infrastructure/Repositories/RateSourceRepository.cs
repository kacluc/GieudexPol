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
            return await _dbSet.FirstOrDefaultAsync(rs => rs.Code == code);
        }
    }
}
