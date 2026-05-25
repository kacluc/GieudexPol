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
    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _mockWalletRepository;
        // Dodano brakujące pole dla mocka serwisu transakcji
        private readonly Mock<ITransactionService> _mockTransactionService;
        private readonly WalletService _walletService;

        public WalletServiceTests()
        {
            _mockWalletRepository = new Mock<IWalletRepository>();
            // Inicjalizacja brakującego mocka
            _mockTransactionService = new Mock<ITransactionService>();
            
            // Przekazanie obu wymaganych obiektów do konstruktora serwisu produkcyjnego
            _walletService = new WalletService(_mockWalletRepository.Object, _mockTransactionService.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var wallet = new Wallet { UserId = 1, CurrencyId = 1, Balance = 100m };
            _mockWalletRepository.Setup(repo => repo.AddAsync(wallet)).Returns(Task.CompletedTask);

            // Act
            await _walletService.AddAsync(wallet);

            // Assert
            _mockWalletRepository.Verify(repo => repo.AddAsync(wallet), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnWallet_WhenWalletExists()
        {
            // Arrange
            var walletId = 1;
            var expectedWallet = new Wallet { Id = walletId, UserId = 1, CurrencyId = 1, Balance = 100m };
            _mockWalletRepository.Setup(repo => repo.GetByIdAsync(walletId)).ReturnsAsync(expectedWallet);

            // Act
            var result = await _walletService.GetByIdAsync(walletId);

            // Assert
            result.Should().BeEquivalentTo(expectedWallet);
            _mockWalletRepository.Verify(repo => repo.GetByIdAsync(walletId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenWalletDoesNotExist()
        {
            // Arrange
            var walletId = 1;
            _mockWalletRepository.Setup(repo => repo.GetByIdAsync(walletId)).ReturnsAsync((Wallet?)null);

            // Act
            var result = await _walletService.GetByIdAsync(walletId);

            // Assert
            result.Should().BeNull();
            _mockWalletRepository.Verify(repo => repo.GetByIdAsync(walletId), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllWallets()
        {
            // Arrange
            var expectedWallets = new List<Wallet>
            {
                new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 100m },
                new Wallet { Id = 2, UserId = 2, CurrencyId = 2, Balance = 50m }
            };
            _mockWalletRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedWallets);

            // Act
            var result = await _walletService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedWallets);
            _mockWalletRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 150m };
            _mockWalletRepository.Setup(repo => repo.UpdateAsync(wallet)).Returns(Task.CompletedTask);

            // Act
            await _walletService.UpdateAsync(wallet);

            // Assert
            _mockWalletRepository.Verify(repo => repo.UpdateAsync(wallet), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 100m };
            _mockWalletRepository.Setup(repo => repo.DeleteAsync(wallet)).Returns(Task.CompletedTask);

            // Act
            await _walletService.DeleteAsync(wallet);

            // Assert
            _mockWalletRepository.Verify(repo => repo.DeleteAsync(wallet), Times.Once);
        }

        [Fact]
        public async Task GetUserWalletsAsync_ShouldReturnWallets_WhenUserHasWallets()
        {
            // Arrange
            var userId = 1;
            var expectedWallets = new List<Wallet>
            {
                new Wallet { Id = 1, UserId = userId, CurrencyId = 1, Balance = 100m },
                new Wallet { Id = 2, UserId = userId, CurrencyId = 2, Balance = 50m }
            };
            _mockWalletRepository.Setup(repo => repo.GetUserWalletsAsync(userId)).ReturnsAsync(expectedWallets);

            // Act
            var result = await _walletService.GetUserWalletsAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedWallets);
            _mockWalletRepository.Verify(repo => repo.GetUserWalletsAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserWalletsAsync_ShouldReturnEmptyList_WhenUserHasNoWallets()
        {
            // Arrange
            var userId = 1;
            _mockWalletRepository.Setup(repo => repo.GetUserWalletsAsync(userId)).ReturnsAsync(new List<Wallet>());

            // Act
            var result = await _walletService.GetUserWalletsAsync(userId);

            // Assert
            result.Should().BeEmpty();
            _mockWalletRepository.Verify(repo => repo.GetUserWalletsAsync(userId), Times.Once);
        }
    }
}
