using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface ICurrencyService : IService<Currency>
    {
        Task<Currency> GetBySymbolAsync(string symbol);
    }
}