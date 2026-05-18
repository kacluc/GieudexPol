using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<ExchangeRateChartPointDto>> GetRatesForChartAsync(
            string currencySymbol,
            string sourceCode,
            DateTime from,
            DateTime to)
        {
            return await _exchangeRateRepository.GetRatesForChartAsync(currencySymbol, sourceCode, from, to);
        }

        public async Task<ExchangeRateChartResponseDto> GetChartDataAsync(
            string currencyCode,
            string sourceCode,
            DateTime from,
            DateTime to)
        {
            var points = await _exchangeRateRepository.GetChartDataAsync(currencyCode, sourceCode, from, to);

            return new ExchangeRateChartResponseDto
            {
                CurrencyCode = currencyCode,
                SourceCode = sourceCode,
                From = from.Date,
                To = to.Date,
                Points = points.ToList()
            };
        }

        public async Task<IEnumerable<ExchangeRateTableRowDto>> GetLatestRatesAsync(string sourceCode)
        {
            return await _exchangeRateRepository.GetLatestRatesAsync(sourceCode);
        }
    }
}
