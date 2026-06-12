using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IOrderMatchingService
    {
        Task<IReadOnlyList<TradeExecution>> MatchAsync(
            Order incomingOrder,
            CancellationToken cancellationToken = default);
    }
}
