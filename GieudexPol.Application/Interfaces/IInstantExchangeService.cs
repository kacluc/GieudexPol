using GieudexPol.Application.DTOs;

namespace GieudexPol.Application.Interfaces
{
    public interface IInstantExchangeService
    {
        Task<TradeExecutionResultDto> ExecuteAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken = default);

        Task<ExchangePreviewResultDto> PreviewAsync(
            int userId,
            int fromCurrencyId,
            decimal amountFrom,
            int toCurrencyId,
            CancellationToken cancellationToken = default);
    }
}
