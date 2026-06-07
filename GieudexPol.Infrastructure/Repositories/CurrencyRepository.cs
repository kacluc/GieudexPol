using GieudexPol.Application.Interfaces;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class CurrencyRepository : GenericRepository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Currency?> GetBySymbolAsync(string symbol)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Symbol == symbol);
        }

        public async Task<IReadOnlyList<Currency>> GetTradableCurrenciesAsync()
        {
            var supportedSymbols = TradingCurrencyCatalog.Symbols;

            return await _dbSet
                .AsNoTracking()
                .Where(currency =>
                    currency.IsActive &&
                    supportedSymbols.Contains(currency.Symbol) &&
                    currency.ExchangeRates.Any())
                .OrderBy(currency => currency.Symbol)
                .ToListAsync();
        }
    }
}
