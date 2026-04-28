using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IExchangeRateRepository _exchangeRateRepository;

        public ExchangeRateService(IExchangeRateRepository exchangeRateRepository)
        {
            _exchangeRateRepository = exchangeRateRepository;
        }

        public async Task<ExchangeRate> GetByIdAsync(int id)
        {
            return await _exchangeRateRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ExchangeRate>> GetAllAsync()
        {
            return await _exchangeRateRepository.GetAllAsync();
        }

        public async Task AddAsync(ExchangeRate entity)
        {
            await _exchangeRateRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(ExchangeRate entity)
        {
            await _exchangeRateRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(ExchangeRate entity)
        {
            await _exchangeRateRepository.DeleteAsync(entity);
        }

        public async Task<ExchangeRate> GetByCurrencyPairAsync(string baseCurrencySymbol, string targetCurrencySymbol)
        {
            return await _exchangeRateRepository.GetByCurrencyPairAsync(baseCurrencySymbol, targetCurrencySymbol);
        }
    }
}