using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface IExternalExchangeRateClient
    {
        string SourceCode { get; }
        string SourceName { get; }
        int MaxRangeDays { get; }

        Task<IReadOnlyList<ExternalExchangeRateTableDto>> GetBuySellRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);
    }
}
