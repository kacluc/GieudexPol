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
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly TransactionService _transactionService;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IWalletRepository> _mockWalletRepository;
        private readonly Mock<ITransactionFeeRepository> _mockTransactionFeeRepository;

        public TransactionServiceTests()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockWalletRepository = new Mock<IWalletRepository>();
            _mockTransactionFeeRepository = new Mock<ITransactionFeeRepository>();

            _transactionService = new TransactionService(
                _mockTransactionRepository.Object,
                _mockUserRepository.Object,
                _mockWalletRepository.Object,
                _mockTransactionFeeRepository.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTransaction_WhenTransactionExists()
        {
            // Arrange
            var transactionId = 1;
            var expectedTransaction = new Transaction { Id = transactionId, SenderId = 1, ReceiverId = 2, CurrencyId = 1, Amount = 10m };
            _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId)).ReturnsAsync(expectedTransaction);

            // Act
            var result = await _transactionService.GetByIdAsync(transactionId);

            // Assert
            result.Should().BeEquivalentTo(expectedTransaction);
            _mockTransactionRepository.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // Arrange
            var transactionId = 1;
            _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId)).ReturnsAsync((Transaction)null);

            // Act
            var result = await _transactionService.GetByIdAsync(transactionId);

            // Assert
            result.Should().BeNull();
            _mockTransactionRepository.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTransactions()
        {
            // Arrange
            var expectedTransactions = new List<Transaction>
            {
                new Transaction { Id = 1, SenderId = 1, ReceiverId = 2, CurrencyId = 1, Amount = 10m },
                new Transaction { Id = 2, SenderId = 2, ReceiverId = 1, CurrencyId = 2, Amount = 20m }
            };
            _mockTransactionRepository.Setup(repo => repo.GetByUserIdAsync(0)).ReturnsAsync(expectedTransactions);

            // Act
            var result = await _transactionService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedTransactions);
            _mockTransactionRepository.Verify(repo => repo.GetByUserIdAsync(0), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var transaction = new Transaction { SenderId = 1, ReceiverId = 2, CurrencyId = 1, Amount = 10m };
            _mockTransactionRepository.Setup(repo => repo.AddAsync(transaction)).Returns(Task.CompletedTask);

            // Act
            await _transactionService.AddAsync(transaction);

            // Assert
            _mockTransactionRepository.Verify(repo => repo.AddAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
        {
            // Arrange
            var transaction = new Transaction { Id = 1, SenderId = 1, ReceiverId = 2, CurrencyId = 1, Amount = 15m };
            _mockTransactionRepository.Setup(repo => repo.UpdateAsync(transaction)).Returns(Task.CompletedTask);

            // Act
            await _transactionService.UpdateAsync(transaction);

            // Assert
            _mockTransactionRepository.Verify(repo => repo.UpdateAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var transaction = new Transaction { Id = 1, SenderId = 1, ReceiverId = 2, CurrencyId = 1, Amount = 10m };
            _mockTransactionRepository.Setup(repo => repo.DeleteAsync(transaction.Id)).Returns(Task.CompletedTask);

            // Act
            await _transactionService.DeleteAsync(transaction);

            // Assert
            _mockTransactionRepository.Verify(repo => repo.DeleteAsync(transaction.Id), Times.Once);
        }

        [Fact]
        public async Task GetUserTransactionsAsync_ShouldReturnTransactions_WhenUserHasTransactions()
        {
            // Arrange
            var userId = 1;
            var expectedTransactions = new List<Transaction>
            {
                new Transaction { Id = 1, SenderId = userId, ReceiverId = 2, CurrencyId = 1, Amount = 10m },
                new Transaction { Id = 2, SenderId = userId, ReceiverId = 2, CurrencyId = 2, Amount = 20m }
            };
            _mockTransactionRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(expectedTransactions);

            // Act
            var result = await _transactionService.GetUserTransactions(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedTransactions);
            _mockTransactionRepository.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
        }
    }
}
