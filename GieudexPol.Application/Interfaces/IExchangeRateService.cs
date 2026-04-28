using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IExchangeRateService : IService<ExchangeRate>
    {
        Task<ExchangeRate> GetByCurrencyPairAsync(string baseCurrencySymbol, string targetCurrencySymbol);
    }
}