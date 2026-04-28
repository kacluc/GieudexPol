using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class ExchangeRateRepository : GenericRepository<ExchangeRate>, IExchangeRateRepository
    {
        public async Task<ExchangeRate> GetByCurrencyPairAsync(string baseCurrencySymbol, string targetCurrencySymbol)
        {
            return await Task.FromResult(_data.FirstOrDefault(er => er.Currency.Symbol == baseCurrencySymbol)); // Simplified
        }
    }
}