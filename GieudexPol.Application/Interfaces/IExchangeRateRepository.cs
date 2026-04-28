using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IExchangeRateRepository : IRepository<ExchangeRate>
    {
        Task<ExchangeRate> GetByCurrencyPairAsync(string baseCurrencySymbol, string targetCurrencySymbol);
    }
}