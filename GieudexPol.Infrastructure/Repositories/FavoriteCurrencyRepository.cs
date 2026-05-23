using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure.Repositories
{
    public class FavoriteCurrencyRepository : IFavoriteCurrencyRepository
    {
        private readonly ApplicationDbContext _context;

        public FavoriteCurrencyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FavoriteCurrency>> GetFavoritesAsync()
        {
            return await _context.FavoriteCurrencies.ToListAsync();
        }

        public async Task AddAsync(FavoriteCurrency favorite)
        {
            _context.FavoriteCurrencies.Add(favorite);

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(string currencyCode)
        {
            var favorite = await _context.FavoriteCurrencies
                .FirstOrDefaultAsync(x => x.CurrencyCode == currencyCode);

            if (favorite != null)
            {
                _context.FavoriteCurrencies.Remove(favorite);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string currencyCode)
        {
            return await _context.FavoriteCurrencies
                .AnyAsync(x => x.CurrencyCode == currencyCode);
        }
    }
}