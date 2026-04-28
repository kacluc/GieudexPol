using GieudexPol.Domain.Entities;

namespace GieudexPol.Application.Interfaces
{
    public interface ITransactionService : IService<Transaction>
    {
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId);
    }
}