using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly ApplicationDbContext _context; 
        private readonly DbSet<Wallet> _dbSet;

        public WalletRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Wallet>();
        }

        public async Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId)
        {
            return await _dbSet
                .Include(w => w.Currency)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<Wallet?> GetWalletByIdAsync(int walletId)
        {
            return await _dbSet.FindAsync(walletId);
        }

        public async Task<Wallet?> GetUserWalletAsync(int userId, int currencyId)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyId == currencyId);
        }

        public async Task DebitWalletBalanceAsync(int walletId, decimal amount)
        {
            var wallet = await GetWalletByIdAsync(walletId);
            if (wallet == null) throw new KeyNotFoundException($"Wallet with ID {walletId} not found.");
            wallet.Debit(amount); 
            await _context.SaveChangesAsync();
        }

        public async Task CreditWalletBalanceAsync(int walletId, decimal amount)
        {
            var wallet = await GetWalletByIdAsync(walletId);
            if (wallet == null) throw new KeyNotFoundException($"Wallet with ID {walletId} not found.");
            wallet.Credit(amount); 
            await _context.SaveChangesAsync();
        }

        public async Task ExecuteBalanceOperationAsync(
            int walletId,
            decimal balanceChange,
            Transaction transaction)
        {
            _context.ChangeTracker.Clear();
            var wallet = await _dbSet.SingleOrDefaultAsync(item => item.Id == walletId)
                ?? throw new KeyNotFoundException($"Wallet with ID {walletId} not found.");

            if (balanceChange >= 0)
            {
                wallet.Credit(balanceChange);
            }
            else
            {
                wallet.Debit(decimal.Abs(balanceChange));
            }

            var persistedTransaction = CopyTransaction(transaction);
            await _context.Transactions.AddAsync(persistedTransaction);
            await _context.SaveChangesAsync();
            transaction.Id = persistedTransaction.Id;
        }

        public async Task ExecuteTradeAsync(
            Wallet fromWallet,
            decimal amountFrom,
            Wallet toWallet,
            decimal amountTo,
            Transaction sellTransaction,
            Transaction buyTransaction)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var persistedFromWallet = await _dbSet.SingleAsync(wallet => wallet.Id == fromWallet.Id);
                var persistedToWallet = await _dbSet.SingleAsync(wallet => wallet.Id == toWallet.Id);
                persistedFromWallet.Debit(amountFrom);
                persistedToWallet.Credit(amountTo);

                await _context.Transactions.AddRangeAsync(
                    CopyTransaction(sellTransaction),
                    CopyTransaction(buyTransaction));
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            });
        }

        public async Task ExecuteTransferAsync(
            int senderWalletId,
            int receiverUserId,
            int currencyId,
            decimal amount,
            decimal fee,
            Transaction transaction)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();
                await using var databaseTransaction = await _context.Database.BeginTransactionAsync();

                var senderWallet = await _dbSet.SingleOrDefaultAsync(wallet =>
                    wallet.Id == senderWalletId && wallet.CurrencyId == currencyId);
                if (senderWallet == null)
                {
                    throw new InvalidOperationException("Sender wallet was not found.");
                }

                var receiverWallet = await _dbSet.SingleOrDefaultAsync(wallet =>
                    wallet.UserId == receiverUserId && wallet.CurrencyId == currencyId);
                if (receiverWallet == null)
                {
                    receiverWallet = new Wallet
                    {
                        UserId = receiverUserId,
                        CurrencyId = currencyId,
                        Balance = 0m
                    };
                    await _dbSet.AddAsync(receiverWallet);
                }

                senderWallet.Debit(amount + fee);
                receiverWallet.Credit(amount);

                var persistedTransaction = CopyTransaction(transaction);
                await _context.Transactions.AddAsync(persistedTransaction);
                await _context.SaveChangesAsync();
                await databaseTransaction.CommitAsync();

                transaction.Id = persistedTransaction.Id;
            });
        }

        public async Task<Wallet?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(w => w.Currency)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task AddAsync(Wallet entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Wallet entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Wallet>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task DeleteAsync(Wallet entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        private static Transaction CopyTransaction(Transaction transaction)
        {
            return new Transaction
            {
                SenderId = transaction.SenderId,
                ReceiverId = transaction.ReceiverId,
                CurrencyId = transaction.CurrencyId,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                AppliedFee = transaction.AppliedFee,
                Status = transaction.Status,
                Timestamp = transaction.Timestamp,
                TransactionFeeId = transaction.TransactionFeeId,
                TradeExecutionId = transaction.TradeExecutionId
            };
        }
    }
}
