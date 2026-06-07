using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GieudexPol.Tests
{
    public class WhaleRankingRepositoryTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly WhaleRankingRepository _repository;

        public WhaleRankingRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);
            _repository = new WhaleRankingRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private async Task SeedUsersAsync(params int[] userIds)
        {
            var users = userIds.Select(id => new User
            {
                Id = id,
                Username = $"User{id}"
            });

            await _context.Users.AddRangeAsync(users);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllWhaleRankings()
        {
            // Arrange
            var whaleRankings = new List<WhaleRanking>
            {
                new WhaleRanking { Id = 1, UserId = 1, TotalPortfolioValue = 1000, Rank = 1, LastUpdated = DateTime.UtcNow },
                new WhaleRanking { Id = 2, UserId = 2, TotalPortfolioValue = 2000, Rank = 2, LastUpdated = DateTime.UtcNow }
            };

            await SeedUsersAsync(1, 2);
            await _context.WhaleRankings.AddRangeAsync(whaleRankings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsWhaleRanking_WhenWhaleRankingExists()
        {
            // Arrange
            var whaleRanking = new WhaleRanking { Id = 1, UserId = 1, TotalPortfolioValue = 1000, Rank = 1, LastUpdated = DateTime.UtcNow };

            await SeedUsersAsync(1);
            await _context.WhaleRankings.AddAsync(whaleRanking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenWhaleRankingDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsWhaleRanking_WhenWhaleRankingExists()
        {
            // Arrange
            var whaleRanking = new WhaleRanking { Id = 1, UserId = 1, TotalPortfolioValue = 1000, Rank = 1, LastUpdated = DateTime.UtcNow };

            await SeedUsersAsync(1);
            await _context.WhaleRankings.AddAsync(whaleRanking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNull_WhenWhaleRankingDoesNotExist()
        {
            // Act
            var result = await _repository.GetByUserIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_AddsWhaleRanking()
        {
            // Arrange
            var whaleRanking = new WhaleRanking { Id = 1, UserId = 1, TotalPortfolioValue = 1000, Rank = 1, LastUpdated = DateTime.UtcNow };

            // Act
            await _repository.AddAsync(whaleRanking);

            // Assert
            var result = await _context.WhaleRankings.FirstOrDefaultAsync(wr => wr.Id == 1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesWhaleRanking()
        {
            // Arrange
            var whaleRanking = new WhaleRanking { Id = 1, UserId = 1, TotalPortfolioValue = 1000, Rank = 1, LastUpdated = DateTime.UtcNow };

            await _context.WhaleRankings.AddAsync(whaleRanking);
            await _context.SaveChangesAsync();

            whaleRanking.TotalPortfolioValue = 2000;

            // Act
            await _repository.UpdateAsync(whaleRanking);

            // Assert
            var result = await _context.WhaleRankings.FirstOrDefaultAsync(wr => wr.Id == 1);
            Assert.NotNull(result);
            Assert.Equal(2000, result.TotalPortfolioValue);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhaleRanking()
        {
            // Arrange
            var whaleRanking = new WhaleRanking { Id = 1, UserId = 1, TotalPortfolioValue = 1000, Rank = 1, LastUpdated = DateTime.UtcNow };

            await _context.WhaleRankings.AddAsync(whaleRanking);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(whaleRanking);

            // Assert
            var result = await _context.WhaleRankings.FirstOrDefaultAsync(wr => wr.Id == 1);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTopWhalesAsync_ReturnsTopWhales()
        {
            // Arrange
            var whaleRankings = new List<WhaleRanking>
            {
                new WhaleRanking { Id = 1, UserId = 1, TotalPortfolioValue = 1000, Rank = 1, LastUpdated = DateTime.UtcNow },
                new WhaleRanking { Id = 2, UserId = 2, TotalPortfolioValue = 2000, Rank = 2, LastUpdated = DateTime.UtcNow },
                new WhaleRanking { Id = 3, UserId = 3, TotalPortfolioValue = 3000, Rank = 3, LastUpdated = DateTime.UtcNow }
            };

            await SeedUsersAsync(1, 2, 3);
            await _context.WhaleRankings.AddRangeAsync(whaleRankings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTopWhalesAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(3000, result.First().TotalPortfolioValue);
        }

        [Fact]
        public async Task GetAllAsync_DoesNotReturnDevelopmentUser()
        {
            var regularUser = new User { Id = 1, Username = "user@gieudexpol.local" };
            var developmentUser = new User
            {
                Id = 2,
                Username = DevelopmentIdentity.UserEmail
            };

            await _context.Users.AddRangeAsync(regularUser, developmentUser);
            await _context.WhaleRankings.AddRangeAsync(
                new WhaleRanking
                {
                    Id = 1,
                    UserId = regularUser.Id,
                    TotalPortfolioValue = 1000,
                    Rank = 2,
                    LastUpdated = DateTime.UtcNow
                },
                new WhaleRanking
                {
                    Id = 2,
                    UserId = developmentUser.Id,
                    TotalPortfolioValue = 2000,
                    Rank = 1,
                    LastUpdated = DateTime.UtcNow
                });
            await _context.SaveChangesAsync();

            var result = (await _repository.GetAllAsync()).ToList();

            var ranking = Assert.Single(result);
            Assert.Equal(regularUser.Id, ranking.UserId);
        }

        [Fact]
        public async Task RefreshRankingAsync_RefreshesRanking()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "User1" },
                new User { Id = 2, Username = "User2" }
            };

            var currencies = new List<Currency>
            {
                new Currency { Id = 1, Symbol = "PLN", Name = "Polish Zloty", IsActive = true },
                new Currency { Id = 2, Symbol = "USD", Name = "US Dollar", IsActive = true }
            };

            var wallets = new List<Wallet>
            {
                new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 1000 },
                new Wallet { Id = 2, UserId = 1, CurrencyId = 2, Balance = 100 },
                new Wallet { Id = 3, UserId = 2, CurrencyId = 1, Balance = 2000 },
                new Wallet { Id = 4, UserId = 2, CurrencyId = 2, Balance = 200 }
            };

            var exchangeRates = new List<ExchangeRate>
            {
                new ExchangeRate { Id = 1, CurrencyId = 1, RateSourceId = 1, BuyPrice = 1, SellPrice = 1, EffectiveDate = DateTime.UtcNow },
                new ExchangeRate { Id = 2, CurrencyId = 2, RateSourceId = 1, BuyPrice = 4, SellPrice = 4, EffectiveDate = DateTime.UtcNow }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.Currencies.AddRangeAsync(currencies);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.ExchangeRates.AddRangeAsync(exchangeRates);
            await _context.SaveChangesAsync();

            // Act
            await _repository.RefreshRankingAsync();

            // Assert
            var result = await _context.WhaleRankings
                .OrderBy(wr => wr.Rank)
                .ToListAsync();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(1, result.First().Rank);
            Assert.Equal(2, result.Last().Rank);
        }
    }
}
