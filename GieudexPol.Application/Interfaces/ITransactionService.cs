using GieudexPol.Application.DTOs;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace GieudexPol.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task AddAsync(Transaction entity);
        Task UpdateAsync(Transaction entity);
        Task DeleteAsync(Transaction entity);
        Task<Transaction> CreateTransfer(int senderId, TransferRequest request);
        Task<PaginatedResult<TransactionDto>> GetUserTransactions(
            int userId,
            int pageNumber,
            int pageSize,
            string? transactionType,
            int? currencyId,
            DateTime? startDate,
            DateTime? endDate);
    }
}
