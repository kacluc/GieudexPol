using Xunit;
using Moq;
using FluentAssertions;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using GieudexPol.Application.DTOs;

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
            _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId)).ReturnsAsync((Transaction?)null);

            // Act
            var result = await _transactionService.GetByIdAsync(transactionId);

            // Assert
            result.Should().BeNull();
            _mockTransactionRepository.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList()
        {
            // Arrange
            _mockTransactionRepository.Setup(repo => repo.GetByUserIdAsync(0, It.IsAny<int>(), It.IsAny<int>(), null, null, null, null))
                                    .ReturnsAsync(Enumerable.Empty<Transaction>());

            // Act
            var result = await _transactionService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
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
        public async Task CreateTransfer_WithActiveFee_UpdatesWalletsAndCompletesTransaction()
        {
            // Arrange
            var sender = new User { Id = 4, Username = "robert.kubica@gieudexpol.local" };
            var receiver = new User { Id = 7, Username = "zenek.martyniuk@gieudexpol.local" };
            var senderWallet = new Wallet { Id = 1, UserId = sender.Id, CurrencyId = 1, Balance = 1000m };
            var receiverWallet = new Wallet { Id = 2, UserId = receiver.Id, CurrencyId = 1, Balance = 500m };
            var currency = new Currency { Id = 1, Symbol = "USD" };
            var fee = new TransactionFee
            {
                Id = Guid.NewGuid(),
                Type = "Transfer",
                FeePercentage = 0.25m,
                FlatFee = 0m,
                IsActive = true
            };

            _mockTransactionFeeRepository
                .Setup(repo => repo.GetActiveTransactionFeeByTypeAsync("Transfer"))
                .ReturnsAsync(fee);
            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(sender.Id))
                .ReturnsAsync(sender);
            _mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync(receiver.Username))
                .ReturnsAsync(receiver);
            _mockWalletRepository
                .Setup(repo => repo.GetUserWalletAsync(sender.Id, currency.Id))
                .ReturnsAsync(senderWallet);
            _mockWalletRepository
                .Setup(repo => repo.ExecuteTransferAsync(
                    senderWallet.Id,
                    receiver.Id,
                    currency.Id,
                    100m,
                    0.25m,
                    It.IsAny<Transaction>()))
                .Callback<int, int, int, decimal, decimal, Transaction>(
                    (_, _, _, amount, appliedFee, transaction) =>
                    {
                        senderWallet.Balance -= amount + appliedFee;
                        receiverWallet.Balance += amount;
                        transaction.Id = 123;
                    })
                .Returns(Task.CompletedTask);

            var request = new TransferRequest
            {
                ReceiverUsername = receiver.Username,
                Amount = 100m,
                CurrencyId = currency.Id
            };

            // Act
            var result = await _transactionService.CreateTransfer(sender.Id, request);

            // Assert
            result.Id.Should().Be(123);
            result.Status.Should().Be("Completed");
            result.AppliedFee.Should().Be(0.25m);
            senderWallet.Balance.Should().Be(899.75m);
            receiverWallet.Balance.Should().Be(600m);
            _mockWalletRepository.Verify(repo => repo.ExecuteTransferAsync(
                senderWallet.Id,
                receiver.Id,
                currency.Id,
                100m,
                0.25m,
                It.Is<Transaction>(transaction =>
                    transaction.SenderId == sender.Id &&
                    transaction.ReceiverId == receiver.Id &&
                    transaction.Status == "Completed")),
                Times.Once);
            _mockTransactionRepository.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Never);
            _mockTransactionRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        }

        [Fact]
        public async Task GetUserTransactions_ShouldReturnPaginatedResult()
        {
            // Arrange
            var userId = 1;
            var pageNumber = 1;
            var pageSize = 10;
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, SenderId = userId, ReceiverId = 2, Amount = 100m, CurrencyId = 1, Status = "Completed", TransactionType = "Transfer", AppliedFee = 1m, Timestamp = DateTime.UtcNow, Sender = new User { Id = userId, Username = "user1" }, Receiver = new User { Id = 2, Username = "user2" }, Currency = new Currency { Id = 1, Symbol = "USD" } },
                new Transaction { Id = 2, SenderId = 3, ReceiverId = userId, Amount = 50m, CurrencyId = 1, Status = "Completed", TransactionType = "Deposit", AppliedFee = 0m, Timestamp = DateTime.UtcNow, Sender = new User { Id = 3, Username = "user3" }, Receiver = new User { Id = userId, Username = "user1" }, Currency = new Currency { Id = 1, Symbol = "USD" } }
            };
            var totalRecords = 2;

            _mockTransactionRepository.Setup(repo => repo.GetByUserIdAsync(
                userId, pageNumber, pageSize, null, null, null, null))
                .ReturnsAsync(transactions);
            _mockTransactionRepository.Setup(repo => repo.GetTotalRecordsByUserIdAsync(
                userId, null, null, null, null))
                .ReturnsAsync(totalRecords);

            // Act
            var result = await _transactionService.GetUserTransactions(
                userId, pageNumber, pageSize, null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(totalRecords);
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
            result.Items.First().SenderUsername.Should().Be("user1");
        }

        [Fact]
        public async Task GetUserTransactions_ShouldFilterByType()
        {
            // Arrange
            var userId = 1;
            var pageNumber = 1;
            var pageSize = 10;
            var transactionType = "Transfer";
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, SenderId = userId, ReceiverId = 2, Amount = 100m, CurrencyId = 1, Status = "Completed", TransactionType = "Transfer", AppliedFee = 1m, Timestamp = DateTime.UtcNow, Sender = new User { Id = userId, Username = "user1" }, Receiver = new User { Id = 2, Username = "user2" }, Currency = new Currency { Id = 1, Symbol = "USD" } }
            };
            var totalRecords = 1;

            _mockTransactionRepository.Setup(repo => repo.GetByUserIdAsync(
                userId, pageNumber, pageSize, transactionType, null, null, null))
                .ReturnsAsync(transactions);
            _mockTransactionRepository.Setup(repo => repo.GetTotalRecordsByUserIdAsync(
                userId, transactionType, null, null, null))
                .ReturnsAsync(totalRecords);

            // Act
            var result = await _transactionService.GetUserTransactions(
                userId, pageNumber, pageSize, transactionType, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().TransactionType.Should().Be(transactionType);
            result.TotalCount.Should().Be(totalRecords);
        }

        [Fact]
        public async Task GetUserTransactions_ShouldFilterByCurrency()
        {
            // Arrange
            var userId = 1;
            var pageNumber = 1;
            var pageSize = 10;
            var currencyId = 1;
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, SenderId = userId, ReceiverId = 2, Amount = 100m, CurrencyId = 1, Status = "Completed", TransactionType = "Transfer", AppliedFee = 1m, Timestamp = DateTime.UtcNow, Sender = new User { Id = userId, Username = "user1" }, Receiver = new User { Id = 2, Username = "user2" }, Currency = new Currency { Id = 1, Symbol = "USD" } }
            };
            var totalRecords = 1;

            _mockTransactionRepository.Setup(repo => repo.GetByUserIdAsync(
                userId, pageNumber, pageSize, null, currencyId, null, null))
                .ReturnsAsync(transactions);
            _mockTransactionRepository.Setup(repo => repo.GetTotalRecordsByUserIdAsync(
                userId, null, currencyId, null, null))
                .ReturnsAsync(totalRecords);

            // Act
            var result = await _transactionService.GetUserTransactions(
                userId, pageNumber, pageSize, null, currencyId, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().CurrencySymbol.Should().Be("USD");
            result.TotalCount.Should().Be(totalRecords);
        }

        [Fact]
        public async Task GetUserTransactions_ShouldFilterByDateRange()
        {
            // Arrange
            var userId = 1;
            var pageNumber = 1;
            var pageSize = 10;
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, SenderId = userId, ReceiverId = 2, Amount = 100m, CurrencyId = 1, Status = "Completed", TransactionType = "Transfer", AppliedFee = 1m, Timestamp = new DateTime(2023, 6, 15), Sender = new User { Id = userId, Username = "user1" }, Receiver = new User { Id = 2, Username = "user2" }, Currency = new Currency { Id = 1, Symbol = "USD" } }
            };
            var totalRecords = 1;

            _mockTransactionRepository.Setup(repo => repo.GetByUserIdAsync(
                userId, pageNumber, pageSize, null, null, startDate, endDate))
                .ReturnsAsync(transactions);
            _mockTransactionRepository.Setup(repo => repo.GetTotalRecordsByUserIdAsync(
                userId, null, null, startDate, endDate))
                .ReturnsAsync(totalRecords);

            // Act
            var result = await _transactionService.GetUserTransactions(
                userId, pageNumber, pageSize, null, null, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Timestamp.Should().Be(new DateTime(2023, 6, 15));
            result.TotalCount.Should().Be(totalRecords);
        }
    }
}
