using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
        Task AddAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id);
    }
}
