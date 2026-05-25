
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class TransactionFeeRepository : ITransactionFeeRepository
    {
        // In a real application, this would interact with a database context.
        // For now, we\"ll use a simple in-memory collection.
        private readonly List<TransactionFee> _transactionFees = new List<TransactionFee>();

        public async Task<TransactionFee?> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_transactionFees.FirstOrDefault(tf => tf.Id == id));
        }

        public async Task<IEnumerable<TransactionFee>> GetAllAsync()
        {
            return await Task.FromResult(_transactionFees);
        }

        public async Task AddAsync(TransactionFee entity)
        {
            entity.Id = Guid.NewGuid();
            _transactionFees.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(TransactionFee entity)
        {
            var existingFee = _transactionFees.FirstOrDefault(tf => tf.Id == entity.Id);
            if (existingFee != null)
            {
                existingFee.Type = entity.Type;
                existingFee.FeePercentage = entity.FeePercentage;
                existingFee.FlatFee = entity.FlatFee;
                existingFee.IsActive = entity.IsActive;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            _transactionFees.RemoveAll(tf => tf.Id == id);
            await Task.CompletedTask;
        }

        public async Task<TransactionFee?> GetActiveTransactionFeeByTypeAsync(string type)
        {
            return await Task.FromResult(_transactionFees.FirstOrDefault(tf => tf.Type == type && tf.IsActive));
        }
    }
}
