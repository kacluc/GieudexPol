using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class CurrencyRepository : GenericRepository<Currency>, ICurrencyRepository
    {
        public async Task<Currency> GetBySymbolAsync(string symbol)
        {
            return await Task.FromResult(_data.FirstOrDefault(c => c.Symbol == symbol));
        }
    }
}