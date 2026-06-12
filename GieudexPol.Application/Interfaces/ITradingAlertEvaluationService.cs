using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface ITradingAlertEvaluationService
    {
        Task<AlertEvaluationResult> EvaluateAllActiveAlertsAsync(
            CancellationToken cancellationToken = default);

        Task<AlertEvaluationResult> EvaluateAlertAsync(
            int alertId,
            CancellationToken cancellationToken = default);

        Task<AlertEvaluationResult> EvaluatePairAsync(
            int tradingPairId,
            CancellationToken cancellationToken = default);
    }
}
