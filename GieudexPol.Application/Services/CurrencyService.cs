using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;

        public CurrencyService(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public async Task<Currency> GetByIdAsync(int id)
        {
            return await _currencyRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Currency>> GetAllAsync()
        {
            return await _currencyRepository.GetAllAsync();
        }

        public async Task AddAsync(Currency entity)
        {
            await _currencyRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(Currency entity)
        {
            await _currencyRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Currency entity)
        {
            await _currencyRepository.DeleteAsync(entity);
        }

        public async Task<Currency> GetBySymbolAsync(string symbol)
        {
            return await _currencyRepository.GetBySymbolAsync(symbol);
        }
    }
}