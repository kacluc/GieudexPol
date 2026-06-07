using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? transactionType,
            int? currencyId,
            DateTime? startDate,
            DateTime? endDate);
        Task<int> GetTotalRecordsByUserIdAsync(
            int userId,
            string? transactionType,
            int? currencyId,
            DateTime? startDate,
            DateTime? endDate);
        Task AddAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id);
    }
}
