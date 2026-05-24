using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface IExchangeRateSyncService
    {
        Task<NbpSyncResultDto> SyncRatesAsync(
            string sourceCode,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        Task<NbpSyncResultDto> SyncCurrentYearRatesAsync(
            string sourceCode,
            CancellationToken cancellationToken = default);

        Task<NbpSyncResultDto> SyncNbpRatesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);
    }
}
