using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class WhaleRankingRepository : IWhaleRankingRepository
    {
        private readonly ApplicationDbContext _context;

        public WhaleRankingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WhaleRanking>> GetAllAsync()
        {
            return await _context.WhaleRankings
                .Include(wr => wr.User)
                .ToListAsync();
        }

        public async Task<WhaleRanking?> GetByIdAsync(int id)
        {
            return await _context.WhaleRankings
                .Include(wr => wr.User)
                .FirstOrDefaultAsync(wr => wr.Id == id);
        }

        public async Task<WhaleRanking?> GetByUserIdAsync(int userId)
        {
            return await _context.WhaleRankings
                .Include(wr => wr.User)
                .FirstOrDefaultAsync(wr => wr.UserId == userId);
        }

        public async Task AddAsync(WhaleRanking entity)
        {
            await _context.WhaleRankings.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WhaleRanking entity)
        {
            _context.WhaleRankings.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(WhaleRanking entity)
        {
            _context.WhaleRankings.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WhaleRanking>> GetTopWhalesAsync(int topN)
        {
            return await _context.WhaleRankings
                .Include(wr => wr.User)
                .OrderByDescending(wr => wr.TotalPortfolioValue)
                .Take(topN)
                .ToListAsync();
        }

        public async Task RefreshRankingAsync()
        {
            var users = await _context.Users.ToListAsync();
            var whaleRankings = new List<WhaleRanking>();

            foreach (var user in users)
            {
                var wallets = await _context.Wallets
                    .Where(w => w.UserId == user.Id)
                    .Include(w => w.Currency)
                    .ToListAsync();

                decimal totalPortfolioValue = 0;

                foreach (var wallet in wallets)
                {
                    var exchangeRate = await _context.ExchangeRates
                        .Where(er => er.CurrencyId == wallet.CurrencyId)
                        .OrderByDescending(er => er.EffectiveDate)
                        .FirstOrDefaultAsync();

                    if (exchangeRate != null)
                    {
                        totalPortfolioValue += wallet.Balance * exchangeRate.BuyPrice;
                    }
                }

                var whaleRanking = new WhaleRanking
                {
                    UserId = user.Id,
                    User = user,
                    TotalPortfolioValue = totalPortfolioValue,
                    Rank = 0,
                    LastUpdated = DateTime.UtcNow
                };

                whaleRankings.Add(whaleRanking);
            }

            whaleRankings = whaleRankings
                .OrderByDescending(wr => wr.TotalPortfolioValue)
                .ToList();

            for (int i = 0; i < whaleRankings.Count; i++)
            {
                whaleRankings[i].Rank = i + 1;
            }

            _context.WhaleRankings.RemoveRange(_context.WhaleRankings);
            await _context.WhaleRankings.AddRangeAsync(whaleRankings);
            await _context.SaveChangesAsync();
        }
    }
}