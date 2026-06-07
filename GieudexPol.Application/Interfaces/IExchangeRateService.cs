using GieudexPol.Application.DTOs;
using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IExchangeRateService : IService<ExchangeRate>
    {
        Task<ExchangeRate?> GetByCurrencyPairAsync(string baseCurrencySymbol, string targetCurrencySymbol);
        Task<ExchangeRateChartResponseDto> GetChartDataAsync(string currencyCode, string sourceCode, DateTime from, DateTime to);
        Task<IEnumerable<ExchangeRateChartPointDto>> GetRatesForChartAsync(string currencySymbol, string sourceCode, DateTime from, DateTime to);
        Task<IEnumerable<ExchangeRateTableRowDto>> GetLatestRatesAsync(string sourceCode, string? currencyCode = null);
        Task<IReadOnlyList<ExchangeRate>> GetTradingRateCandidatesAsync(
            IReadOnlyCollection<int> currencyIds,
            DateTime oldestAcceptedDate,
            DateTime notAfter);
    }
}
