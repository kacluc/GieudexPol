using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Tests
{
    /// <summary>
/// Test class for the <see cref="WalletService"/> service, using mocked repository interactions.
/// Każdy test jest teraz niezależnie inicjowany, co zapobiega efektom ubocznym między testami.
/// </summary>
public class WalletServiceTests
    {
        [Fact]
        public async Task GetWalletAsync_ReturnsWalletWhenExists()
        {
            // Arrange: Inicjalizacja mocków dla tego testu.
            var mockWalletRepository = new Mock<IWalletRepository>();
            int userId = 2;
            string currencySymbol = "PLN";
            var expectedWallet = new Wallet { Id = 1, UserId = userId, Balance = 500.0 };
            mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(expectedWallet);

            // Tworzenie usługi dla tego testu.
            var walletService = new WalletService(mockWalletRepository.Object);

            // Act
            var result = await walletService.GetWalletAsync(2, "PLN");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500.0, result.Balance);
            mockWalletRepository.Verify(r => r.GetWalletByUserIdAsync(2, "PLN"), Times.Once());
        }

        [Fact]
        public async Task GetWalletAsync_ReturnsNullWhenNotFound()
        {
            // Arrange: Inicjalizacja mocków dla tego testu.
            var mockWalletRepository = new Mock<IWalletRepository>();
            int userId = 99;
            string currencySymbol = "PLN";
            mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync((Wallet)null);

            // Tworzenie usługi dla tego testu.
            var walletService = new WalletService(mockWalletRepository.Object);

            // Act
            var result = await walletService.GetWalletAsync(99, "PLN");

            // Assert
            Assert.Null(result);
            mockWalletRepository.Verify(r => r.GetWalletByUserIdAsync(99, "PLN"), Times.Once());
        }

        [Fact]
        public async Task AddFundsAsync_IncreasesBalanceAndSaves()
        {
            // Arrange: Inicjalizacja mocków dla tego testu.
            var mockWalletRepository = new Mock<IWalletRepository>();
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToAdd = 100.0;
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = 500.0 };

            // Setup retrieval of the current wallet state
            mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Tworzenie usługi dla tego testu.
            var walletService = new WalletService(mockWalletRepository.Object);

            // Act
            await walletService.AddFundsAsync(userId, currencySymbol, amountToAdd);

            // Assert
            var updatedWallet = await mockWalletRepository.GetWalletByUserIdAsync(userId, currencySymbol);
            Assert.Equal(600.0, updatedWallet.Balance); // 500 + 100
            mockWalletRepository.Verify(r => r.SaveWalletAsync(It.IsAny<Wallet>()), Times.Once());
        }

        [Fact]
        public async Task AddFundsAsync_ThrowsExceptionIfInsufficientInitialWallet()
        {
            // Arrange: Inicjalizacja mocków dla tego testu.
            var mockWalletRepository = new Mock<IWalletRepository>();
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToAdd = 100.0;
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = null };

            mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Tworzenie usługi dla tego testu.
            var walletService = new WalletService(mockWalletRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                walletService.AddFundsAsync(userId, currencySymbol, amountToAdd));
        }


        [Fact]
        public async Task WithdrawFundsAsync_DecreasesBalanceAndSaves()
        {
            // Arrange: Inicjalizacja mocków dla tego testu.
            var mockWalletRepository = new Mock<IWalletRepository>();
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToWithdraw = 50.0;
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = 500.0 };

            // Setup retrieval of the current wallet state
            mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Tworzenie usługi dla tego testu.
            var walletService = new WalletService(mockWalletRepository.Object);

            // Act
            await walletService.WithdrawFundsAsync(userId, currencySymbol, amountToWithdraw);

            // Assert
            var updatedWallet = await mockWalletRepository.GetWalletByUserIdAsync(userId, currencySymbol);
            Assert.Equal(450.0, updatedWallet.Balance); // 500 - 50
            mockWalletRepository.Verify(r => r.SaveWalletAsync(It.IsAny<Wallet>()), Times.Once());
        }

        [Fact]
        public async Task WithdrawFundsAsync_ThrowsExceptionIfInsufficientBalance()
        {
            // Arrange: Inicjalizacja mocków dla tego testu.
            var mockWalletRepository = new Mock<IWalletRepository>();
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToWithdraw = 600.0; // Więcej niż dostępne (500)
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = 500.0 };

            mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Tworzenie usługi dla tego testu.
            var walletService = new WalletService(mockWalletRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                walletService.WithdrawFundsAsync(userId, currencySymbol, amountToWithdraw));
        }


        [Fact]
        public async Task WithdrawFundsAsync_ThrowsExceptionIfNoBalance()
        {
            // Arrange: Inicjalizacja mocków dla tego testu.
            var mockWalletRepository = new Mock<IWalletRepository>();
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToWithdraw = 10.0;
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = null };

            mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Tworzenie usługi dla tego testu.
            var walletService = new WalletService(mockWalletRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                walletService.WithdrawFundsAsync(userId, currencySymbol, amountToWithdraw));
        }
    }
}