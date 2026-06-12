using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface IUserTradingAlertService
    {
        Task<IReadOnlyList<UserTradingAlert>> GetUserAlertsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<UserTradingAlert?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task CreateAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            UserTradingAlert alert,
            CancellationToken cancellationToken = default);

        Task<bool> AcknowledgeAsync(
            int alertId,
            int userId,
            CancellationToken cancellationToken = default);
    }
}
