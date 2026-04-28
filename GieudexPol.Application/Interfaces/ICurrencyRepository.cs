using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface ICurrencyRepository : IRepository<Currency>
    {
        Task<Currency> GetBySymbolAsync(string symbol);
    }
}