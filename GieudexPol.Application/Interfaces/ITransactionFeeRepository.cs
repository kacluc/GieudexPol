
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Interfaces
{
    public interface ITransactionFeeRepository
    {
        Task<TransactionFee?> GetByIdAsync(Guid id);
        Task<IEnumerable<TransactionFee>> GetAllAsync();
        Task AddAsync(TransactionFee entity);
        Task UpdateAsync(TransactionFee entity);
        Task DeleteAsync(Guid id);
        Task<TransactionFee?> GetActiveTransactionFeeByTypeAsync(string type);
    }
}
