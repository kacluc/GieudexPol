/// <summary>
/// Klasa testująca usługę zarządzania transakcjami (TransactionService).
/// Testy te weryfikują logikę pobierania, dodawania i modyfikowania rekordów transakcji.
/// </summary>
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Tests
{
    /// <summary>
    /// Zawiera mock dla repozytorium transakcji i instancję usługi do testowania.
    /// </summary>
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly TransactionService _transactionService;

        /// <summary>
        /// Konstruktor, inicjalizujący mocki i usługę pod testowanie.
        /// </summary>
        public TransactionServiceTests()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _transactionService = new TransactionService(_mockTransactionRepository.Object);
        }

        /// <summary>
        /// Testuje pobranie konkretnej transakcji po jej ID (sukces).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ReturnsTransactionWhenExists()
        {
            // Arrange
            var expectedTransaction = new Transaction { Id = 1, UserId = 2 };
            _mockTransactionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedTransaction);

            // Act
            var result = await _transactionService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.UserId);
            _mockTransactionRepository.Verify(r => r.GetByIdAsync(1), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie transakcji, która nie istnieje (powinno zwrócić null).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenNotFound()
        {
            // Arrange
            _mockTransactionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Transaction)null);

            // Act
            var result = await _transactionService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
            _mockTransactionRepository.Verify(r => r.GetByIdAsync(99), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie wszystkich zarejestrowanych transakcji.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ReturnsAllTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, UserId = 2 },
                new Transaction { Id = 2, UserId = 1 }
            };
            _mockTransactionRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockTransactionRepository.Verify(r => r.GetAllAsync(), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa poprawnie wywołuje metodę dodawania nowej transakcji do repozytorium.
        /// </summary>
        [Fact]
        public async Task AddAsync_CallsRepositoryAdd()
        {
            // Arrange
            var newTransaction = new Transaction { UserId = 1 };

            // Act
            await _transactionService.AddAsync(newTransaction);

            // Assert
            _mockTransactionRepository.Verify(r => r.AddAsync(newTransaction), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa poprawnie wywołuje metodę aktualizacji transakcji w repozytorium.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_CallsRepositoryUpdate()
        {
            // Arrange
            var updatedTransaction = new Transaction { Id = 1, UserId = 2 };

            // Act
            await _transactionService.UpdateAsync(updatedTransaction);

            // Assert
            _mockTransactionRepository.Verify(r => r.UpdateAsync(updatedTransaction), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa poprawnie wywołuje metodę usuwania transakcji z repozytorium.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_CallsRepositoryDelete()
        {
            // Arrange
            var transactionToDelete = new Transaction { Id = 1 };

            // Act
            await _transactionService.DeleteAsync(transactionToDelete);

            // Assert
            _mockTransactionRepository.Verify(r => r.DeleteAsync(transactionToDelete), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie wszystkich transakcji przypisanych do konkretnego użytkownika (sukces).
        /// </summary>
        [Fact]
        public async Task GetUserTransactionsAsync_ReturnsByUser()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, UserId = 2 },
                new Transaction { Id = 3, UserId = 2 }
            };
            int targetUserId = 2;
            _mockTransactionRepository.Setup(r => r.GetUserTransactionsAsync(targetUserId))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetUserTransactionsAsync(targetUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockTransactionRepository.Verify(r => r.GetUserTransactionsAsync(targetUserId), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie transakcji dla użytkownika, który nie ma żadnych zapisanych transakcji.
        /// </summary>
        [Fact]
        public async Task GetUserTransactionsAsync_ReturnsEmptyListWhenNoneFound()
        {
            // Arrange
            var emptyList = new List<Transaction>();
            int targetUserId = 99; // Non-existent user ID
            _mockTransactionRepository.Setup(r => r.GetUserTransactionsAsync(targetUserId))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _transactionService.GetUserTransactionsAsync(targetUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockTransactionRepository.Verify(r => r.GetUserTransactionsAsync(targetUserId), Times.Once());
        }
    }
}