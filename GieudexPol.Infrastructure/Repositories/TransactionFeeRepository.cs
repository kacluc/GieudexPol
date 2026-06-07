using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Repositories
{
    public class TransactionFeeRepository : ITransactionFeeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TransactionFee> _transactionFees;

        public TransactionFeeRepository(ApplicationDbContext context)
        {
            _context = context;
            _transactionFees = context.Set<TransactionFee>();
        }

        public async Task<TransactionFee?> GetByIdAsync(Guid id)
        {
            return await _transactionFees.FindAsync(id);
        }

        public async Task<IEnumerable<TransactionFee>> GetAllAsync()
        {
            return await _transactionFees
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(TransactionFee entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            await _transactionFees.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TransactionFee entity)
        {
            _transactionFees.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _transactionFees.FindAsync(id);
            if (entity == null)
            {
                return;
            }

            _transactionFees.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<TransactionFee?> GetActiveTransactionFeeByTypeAsync(string type)
        {
            return await _transactionFees
                .AsNoTracking()
                .FirstOrDefaultAsync(fee => fee.Type == type && fee.IsActive);
        }
    }
}
