using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface IAlertEvaluationService
    {
        Task<AlertEvaluationResult> EvaluateAllActiveAlertsAsync(
            CancellationToken cancellationToken = default);

        Task<AlertEvaluationResult> EvaluateAlertAsync(
            int alertId,
            CancellationToken cancellationToken = default);

        Task<AlertEvaluationResult> EvaluateAlertsForRateAsync(
            string? currencyCode,
            string? rateSourceCode,
            CancellationToken cancellationToken = default);
    }
}
