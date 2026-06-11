using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface IAdminTestExchangeRateService
    {
        Task<IReadOnlyList<AdminTestRateSourceDto>> GetSourcesAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AdminTestExchangeRateDto>> GetRatesAsync(
            string? rateSourceCode,
            int? currencyId,
            string? currencyCode,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken = default);

        Task<AdminTestExchangeRateDto?> GetRateAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<AdminTestExchangeRateDto> CreateRateAsync(
            CreateTestExchangeRateDto request,
            CancellationToken cancellationToken = default);

        Task<AdminTestExchangeRateDto?> UpdateRateAsync(
            int id,
            UpdateTestExchangeRateDto request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteRateAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
