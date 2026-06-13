using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .Include(t => t.Currency)
                .Include(t => t.TradeExecution)
                    .ThenInclude(execution => execution!.TradingPair)
                        .ThenInclude(pair => pair.BaseCurrency)
                .Include(t => t.TradeExecution)
                    .ThenInclude(execution => execution!.TradingPair)
                        .ThenInclude(pair => pair.QuoteCurrency)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? transactionType,
            int? currencyId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.Transactions
                .Where(t => t.SenderId == userId || t.ReceiverId == userId)
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .Include(t => t.Currency)
                .Include(t => t.TradeExecution)
                    .ThenInclude(execution => execution!.TradingPair)
                        .ThenInclude(pair => pair.BaseCurrency)
                .Include(t => t.TradeExecution)
                    .ThenInclude(execution => execution!.TradingPair)
                        .ThenInclude(pair => pair.QuoteCurrency)
                .AsQueryable();

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType == transactionType);
            }

            if (currencyId.HasValue)
            {
                query = query.Where(t => t.CurrencyId == currencyId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Timestamp <= endDate.Value);
            }

            return await query
                .OrderByDescending(t => t.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalRecordsByUserIdAsync(
            int userId,
            string? transactionType,
            int? currencyId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.Transactions
                .Where(t => t.SenderId == userId || t.ReceiverId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType == transactionType);
            }

            if (currencyId.HasValue)
            {
                query = query.Where(t => t.CurrencyId == currencyId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Timestamp <= endDate.Value);
            }

            return await query.CountAsync();
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }
    }
}
