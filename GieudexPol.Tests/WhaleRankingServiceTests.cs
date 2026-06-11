using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GieudexPol.Tests
{
    public class WhaleRankingServiceTests
    {
        private readonly Mock<IWhaleRankingRepository> _mockRepository;
        private readonly WhaleRankingService _service;

        public WhaleRankingServiceTests()
        {
            _mockRepository = new Mock<IWhaleRankingRepository>();
            _service = new WhaleRankingService(_mockRepository.Object);
        }

        private static WhaleRanking CreateRanking(int id, decimal totalPortfolioValue, int rank)
        {
            var user = new User
            {
                Id = id,
                Username = $"user{id}@example.com"
            };

            return new WhaleRanking
            {
                Id = id,
                UserId = user.Id,
                User = user,
                TotalPortfolioValue = totalPortfolioValue,
                Rank = rank
            };
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllWhaleRankings()
        {
            // Arrange
            var whaleRankings = new List<WhaleRanking>
            {
                CreateRanking(1, 1000, 1),
                CreateRanking(2, 2000, 2)
            };

            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(whaleRankings);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("user1", result.First().Username);
            _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsWhaleRanking_WhenWhaleRankingExists()
        {
            // Arrange
            var whaleRanking = CreateRanking(1, 1000, 1);

            _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(whaleRanking);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("user1", result.Username);
            _mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_UsesDisplayName_WhenUserHasOne()
        {
            var whaleRanking = CreateRanking(1, 1000, 1);
            whaleRanking.User.DisplayName = "Kapitan Kapital";
            _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(whaleRanking);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Kapitan Kapital", result.Username);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenWhaleRankingDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((WhaleRanking?)null);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsWhaleRanking_WhenWhaleRankingExists()
        {
            // Arrange
            var whaleRanking = CreateRanking(1, 1000, 1);

            _mockRepository.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync(whaleRanking);

            // Act
            var result = await _service.GetByUserIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            _mockRepository.Verify(repo => repo.GetByUserIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNull_WhenWhaleRankingDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync((WhaleRanking?)null);

            // Act
            var result = await _service.GetByUserIdAsync(1);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(repo => repo.GetByUserIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetTopWhalesAsync_ReturnsTopWhales()
        {
            // Arrange
            var whaleRankings = new List<WhaleRanking>
            {
                CreateRanking(1, 1000, 1),
                CreateRanking(2, 2000, 2),
                CreateRanking(3, 3000, 3)
            };

            _mockRepository.Setup(repo => repo.GetTopWhalesAsync(2)).ReturnsAsync(whaleRankings.Take(2));

            // Act
            var result = await _service.GetTopWhalesAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockRepository.Verify(repo => repo.GetTopWhalesAsync(2), Times.Once);
        }

        [Fact]
        public async Task RefreshRankingAsync_CallsRepositoryRefreshRankingAsync()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.RefreshRankingAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.RefreshRankingAsync();

            // Assert
            _mockRepository.Verify(repo => repo.RefreshRankingAsync(), Times.Once);
        }
    }
}
