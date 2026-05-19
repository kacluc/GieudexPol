using GieudexPol.Application.DTOs;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task AddAsync(Transaction entity);
        Task UpdateAsync(Transaction entity);
        Task DeleteAsync(Transaction entity);
        Task<Transaction> CreateTransfer(TransferRequest request);
        Task<IEnumerable<Transaction>> GetUserTransactions(int userId);
    }
}
