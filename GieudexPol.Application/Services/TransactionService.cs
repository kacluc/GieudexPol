using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<Transaction> GetByIdAsync(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _transactionRepository.GetAllAsync();
        }

        public async Task AddAsync(Transaction entity)
        {
            await _transactionRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(Transaction entity)
        {
            await _transactionRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Transaction entity)
        {
            await _transactionRepository.DeleteAsync(entity);
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId)
        {
            return await _transactionRepository.GetUserTransactionsAsync(userId);
        }
    }
}