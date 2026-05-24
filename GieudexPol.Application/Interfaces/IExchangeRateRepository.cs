using GieudexPol.Application.DTOs;
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface IExchangeRateRepository : IRepository<ExchangeRate>
    {
        Task<ExchangeRate> GetByCurrencyPairAsync(string baseCurrencySymbol, string targetCurrencySymbol);
        Task<IEnumerable<ExchangeRateChartPointDto>> GetChartDataAsync(string currencyCode, string sourceCode, DateTime from, DateTime to);
        Task<IEnumerable<ExchangeRateChartPointDto>> GetRatesForChartAsync(string currencySymbol, string sourceCode, DateTime from, DateTime to);
        Task<IEnumerable<ExchangeRateTableRowDto>> GetLatestRatesAsync(string sourceCode, string? currencyCode = null);
        Task<bool> ExistsAsync(int currencyId, int rateSourceId, DateTime effectiveDate);
    }
}
