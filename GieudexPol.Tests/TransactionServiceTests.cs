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
    public class TransactionServiceTests
    {
        /// <summary>
        /// Testuje pobranie konkretnej transakcji po jej ID (sukces).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ReturnsTransactionWhenExists()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockTransactionRepository = new Mock<ITransactionRepository>();
            var transactionService = new TransactionService(mockTransactionRepository.Object);
            var expectedTransaction = new Transaction { Id = 1, UserId = 2 };
            mockTransactionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedTransaction);

            // Act
            var result = await transactionService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.UserId);
            mockTransactionRepository.Verify(r => r.GetByIdAsync(1), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie transakcji, która nie istnieje (powinno zwrócić null).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenNotFound()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockTransactionRepository = new Mock<ITransactionRepository>();
            var transactionService = new TransactionService(mockTransactionRepository.Object);

            mockTransactionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Transaction)null);

            // Act
            var result = await transactionService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
            mockTransactionRepository.Verify(r => r.GetByIdAsync(99), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie wszystkich zarejestrowanych transakcji.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ReturnsAllTransactions()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockTransactionRepository = new Mock<ITransactionRepository>();
            var transactionService = new TransactionService(mockTransactionRepository.Object);

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, UserId = 2 },
                new Transaction { Id = 2, UserId = 1 }
            };
            mockTransactionRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(transactions);

            // Act
            var result = await transactionService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            mockTransactionRepository.Verify(r => r.GetAllAsync(), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa poprawnie wywołuje metodę dodawania nowej transakcji do repozytorium.
        /// </summary>
        [Fact]
        public async Task AddAsync_CallsRepositoryAdd()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockTransactionRepository = new Mock<ITransactionRepository>();
            var transactionService = new TransactionService(mockTransactionRepository.Object);

            var newTransaction = new Transaction { UserId = 1 };

            // Act
            await transactionService.AddAsync(newTransaction);

            // Assert
            mockTransactionRepository.Verify(r => r.AddAsync(newTransaction), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa poprawnie wywołuje metodę aktualizacji transakcji w repozytorium.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_CallsRepositoryUpdate()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockTransactionRepository = new Mock<ITransactionRepository>();
            var transactionService = new TransactionService(mockTransactionRepository.Object);

            var updatedTransaction = new Transaction { Id = 1, UserId = 2 };

            // Act
            await transactionService.UpdateAsync(updatedTransaction);

            // Assert
            mockTransactionRepository.Verify(r => r.UpdateAsync(updatedTransaction), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa poprawnie wywołuje metodę usuwania transakcji z repozytorium.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_CallsRepositoryDelete()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockTransactionRepository = new Mock<ITransactionRepository>();
            var transactionService = new TransactionService(mockTransactionRepository.Object);

            var transactionToDelete = new Transaction { Id = 1 };

            // Act
            await transactionService.DeleteAsync(transactionToDelete);

            // Assert
            mockTransactionRepository.Verify(r => r.DeleteAsync(transactionToDelete), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie wszystkich transakcji przypisanych do konkretnego użytkownika (sukces).
        /// </summary>
        [Fact]
        public async Task GetUserTransactionsAsync_ReturnsByUser()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockTransactionRepository = new Mock<ITransactionRepository>();
            var transactionService = new TransactionService(mockTransactionRepository.Object);

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, UserId = 2 },
                new Transaction { Id = 3, UserId = 2 }
            };
            int targetUserId = 2;
            mockTransactionRepository.Setup(r => r.GetUserTransactionsAsync(targetUserId))
                .ReturnsAsync(transactions);

            // Act
            var result = await transactionService.GetUserTransactionsAsync(targetUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            mockTransactionRepository.Verify(r => r.GetUserTransactionsAsync(targetUserId), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie transakcji dla użytkownika, który nie ma żadnych zapisanych transakcji.
        /// </summary>
        [Fact]
        public async Task GetUserTransactionsAsync_ReturnsEmptyListWhenNoneFound()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockTransactionRepository = new Mock<ITransactionRepository>();
            var transactionService = new TransactionService(mockTransactionRepository.Object);

            var emptyList = new List<Transaction>();
            int targetUserId = 99; // Non-existent user ID
            mockTransactionRepository.Setup(r => r.GetUserTransactionsAsync(targetUserId))
                .ReturnsAsync(emptyList);

            // Act
            var result = await transactionService.GetUserTransactionsAsync(targetUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            mockTransactionRepository.Verify(r => r.GetUserTransactionsAsync(targetUserId), Times.Once());
        }
    }
}