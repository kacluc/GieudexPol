using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId);
    }
}