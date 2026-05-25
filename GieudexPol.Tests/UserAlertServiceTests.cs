using Xunit;
using Moq;
using FluentAssertions;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GieudexPol.Tests
{
    public class UserAlertServiceTests
    {
        private readonly Mock<IUserAlertRepository> _mockUserAlertRepository;
        private readonly UserAlertService _userAlertService;

        public UserAlertServiceTests()
        {
            _mockUserAlertRepository = new Mock<IUserAlertRepository>();
            _userAlertService = new UserAlertService(_mockUserAlertRepository.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var userAlert = new UserAlert { UserId = 1, CurrencyId = 1, TargetPrice = 1.2m, IsActive = true };
            _mockUserAlertRepository.Setup(repo => repo.AddAsync(userAlert)).Returns(Task.CompletedTask);

            // Act
            await _userAlertService.AddAsync(userAlert);

            // Assert
            _mockUserAlertRepository.Verify(repo => repo.AddAsync(userAlert), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUserAlert_WhenUserAlertExists()
        {
            // Arrange
            var userAlertId = 1;
            var expectedUserAlert = new UserAlert { Id = userAlertId, UserId = 1, CurrencyId = 1, TargetPrice = 1.2m, IsActive = true };
            _mockUserAlertRepository.Setup(repo => repo.GetByIdAsync(userAlertId)).ReturnsAsync(expectedUserAlert);

            // Act
            var result = await _userAlertService.GetByIdAsync(userAlertId);

            // Assert
            result.Should().BeEquivalentTo(expectedUserAlert);
            _mockUserAlertRepository.Verify(repo => repo.GetByIdAsync(userAlertId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserAlertDoesNotExist()
        {
            // Arrange
            var userAlertId = 1;
            _mockUserAlertRepository.Setup(repo => repo.GetByIdAsync(userAlertId)).ReturnsAsync((UserAlert?)null);

            // Act
            var result = await _userAlertService.GetByIdAsync(userAlertId);

            // Assert
            result.Should().BeNull();
            _mockUserAlertRepository.Verify(repo => repo.GetByIdAsync(userAlertId), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUserAlerts()
        {
            // Arrange
            var expectedUserAlerts = new List<UserAlert>
            {
                new UserAlert { Id = 1, UserId = 1, CurrencyId = 1, TargetPrice = 1.2m, IsActive = true },
                new UserAlert { Id = 2, UserId = 2, CurrencyId = 2, TargetPrice = 0.8m, IsActive = false }
            };
            _mockUserAlertRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedUserAlerts);

            // Act
            var result = await _userAlertService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedUserAlerts);
            _mockUserAlertRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
        {
            // Arrange
            var userAlert = new UserAlert { Id = 1, UserId = 1, CurrencyId = 1, TargetPrice = 1.3m, IsActive = false };
            _mockUserAlertRepository.Setup(repo => repo.UpdateAsync(userAlert)).Returns(Task.CompletedTask);

            // Act
            await _userAlertService.UpdateAsync(userAlert);

            // Assert
            _mockUserAlertRepository.Verify(repo => repo.UpdateAsync(userAlert), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var userAlert = new UserAlert { Id = 1, UserId = 1, CurrencyId = 1, TargetPrice = 1.2m, IsActive = true };
            _mockUserAlertRepository.Setup(repo => repo.DeleteAsync(userAlert)).Returns(Task.CompletedTask);

            // Act
            await _userAlertService.DeleteAsync(userAlert);

            // Assert
            _mockUserAlertRepository.Verify(repo => repo.DeleteAsync(userAlert), Times.Once);
        }

        [Fact]
        public async Task GetUserAlertsByUserIdAsync_ShouldReturnAlerts_WhenUserHasAlerts()
        {
            // Arrange
            var userId = 1;
            var expectedAlerts = new List<UserAlert>
            {
                new UserAlert { Id = 1, UserId = userId, CurrencyId = 1, TargetPrice = 1.2m, IsActive = true },
                new UserAlert { Id = 2, UserId = userId, CurrencyId = 2, TargetPrice = 0.8m, IsActive = false }
            };
            _mockUserAlertRepository.Setup(repo => repo.GetUserAlertsByUserIdAsync(userId)).ReturnsAsync(expectedAlerts);

            // Act
            var result = await _userAlertService.GetUserAlertsByUserIdAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedAlerts);
            _mockUserAlertRepository.Verify(repo => repo.GetUserAlertsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserAlertsByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoAlerts()
        {
            // Arrange
            var userId = 1;
            _mockUserAlertRepository.Setup(repo => repo.GetUserAlertsByUserIdAsync(userId)).ReturnsAsync(new List<UserAlert>());

            // Act
            var result = await _userAlertService.GetUserAlertsByUserIdAsync(userId);

            // Assert
            result.Should().BeEmpty();
            _mockUserAlertRepository.Verify(repo => repo.GetUserAlertsByUserIdAsync(userId), Times.Once);
        }
    }
}
