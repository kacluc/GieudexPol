using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    // Przywrócono brakującą definicję klasy oraz implementację interfejsu
    public class WalletRepository : IWalletRepository
    {
        // Założyłem standardową nazwę DbContextu. Jeśli w projekcie nazywa się inaczej (np. AppDbContext), zmień ją poniżej.
        private readonly ApplicationDbContext _context; 
        private readonly DbSet<Wallet> _dbSet;

        public WalletRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Wallet>();
        }

        // Dodano brakującą metodę z Twojego interfejsu IWalletRepository
        public async Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId)
        {
            return await _dbSet
                .Include(w => w.Currency)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        // Prywatna lub publiczna metoda pomocnicza używana w kodzie poniżej
        public async Task<Wallet> GetWalletByIdAsync(int walletId)
        {
            return await _dbSet.FindAsync(walletId);
        }

        public async Task<Wallet> GetWalletByUserIdAndCurrencyIdAsync(int userId, int currencyId)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyId == currencyId);
        }

        /// <summary>
        /// Atomically debits the wallet balance and saves changes to the database.
        /// </summary>
        public async Task DebitWalletBalanceAsync(int walletId, decimal amount)
        {
            var wallet = await GetWalletByIdAsync(walletId);
            if (wallet == null) throw new KeyNotFoundException($"Wallet with ID {walletId} not found.");

            // Wywołanie metody domenowej walidującej stan konta
            wallet.Debit(amount); 

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atomically credits the wallet balance and saves changes to the database.
        /// </summary>
        public async Task CreditWalletBalanceAsync(int walletId, decimal amount)
        {
            var wallet = await GetWalletByIdAsync(walletId);
            if (wallet == null) throw new KeyNotFoundException($"Wallet with ID {walletId} not found.");

            // Wywołanie metody domenowej
            wallet.Credit(amount); 

            await _context.SaveChangesAsync();
        }

        public async Task<Wallet> GetByIdAsync(int id)
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

    }
}
