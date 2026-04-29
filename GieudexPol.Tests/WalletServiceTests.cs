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
/// </summary>
public class WalletServiceTests
    {
    private readonly Mock<IWalletRepository> _mockWalletRepository;
    private readonly WalletService _walletService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalletServiceTests"/> class.
    /// Sets up mock dependencies for testing purposes.
    /// </summary>
    public WalletServiceTests()
        {
            _mockWalletRepository = new Mock<IWalletRepository>();
            _walletService = new WalletService(_mockWalletRepository.Object);
        }

        /// <summary>
        /// Tests that GetWalletAsync returns the expected wallet when a user ID and currency are valid.
        /// </summary>
        [Fact]
        public async Task GetWalletAsync_ReturnsWalletWhenExists()
        {
            // Arrange
            var expectedWallet = new Wallet { Id = 1, UserId = 2, Balance = 500.0 };
            _mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(expectedWallet);

            // Act
            var result = await _walletService.GetWalletAsync(2, "PLN");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500.0, result.Balance);
            _mockWalletRepository.Verify(r => r.GetWalletByUserIdAsync(2, "PLN"), Times.Once());
        }

        /// <summary>
        /// Tests that GetWalletAsync returns null when the wallet is not found for the given user and currency.
        /// </summary>
        [Fact]
        public async Task GetWalletAsync_ReturnsNullWhenNotFound()
        {
            // Arrange
            _mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((Wallet)null);

            // Act
            var result = await _walletService.GetWalletAsync(99, "PLN");

            // Assert
            Assert.Null(result);
            _mockWalletRepository.Verify(r => r.GetWalletByUserIdAsync(99, "PLN"), Times.Once());
        }

        /// <summary>
        /// Tests that calling AddFundsAsync correctly retrieves the wallet, increases the balance, and saves the updated state.
        /// </summary>
        [Fact]
        public async Task AddFundsAsync_IncreasesBalanceAndSaves()
        {
            // Arrange
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToAdd = 100.0;
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = 500.0 };

            // Setup retrieval of the current wallet state
            _mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Act
            await _walletService.AddFundsAsync(userId, currencySymbol, amountToAdd);

            // Assert
            var updatedWallet = await _mockWalletRepository.GetWalletByUserIdAsync(userId, currencySymbol);
            Assert.Equal(600.0, updatedWallet.Balance); // 500 + 100
            _mockWalletRepository.Verify(r => r.SaveWalletAsync(It.IsAny<Wallet>()), Times.Once());
        }

        /// <summary>
        /// Tests that AddFundsAsync throws an InvalidOperationException if the initial wallet balance is null.
        /// </summary>
        [Fact]
        public async Task AddFundsAsync_ThrowsExceptionIfInsufficientInitialWallet()
        {
            // Arrange
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToAdd = 100.0;
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = null };

            _mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _walletService.AddFundsAsync(userId, currencySymbol, amountToAdd));
        }


        /// <summary>
        /// Tests that calling WithdrawFundsAsync correctly decreases the balance and saves the updated state.
        /// </summary>
        [Fact]
        public async Task WithdrawFundsAsync_DecreasesBalanceAndSaves()
        {
            // Arrange
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToWithdraw = 50.0;
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = 500.0 };

            // Setup retrieval of the current wallet state
            _mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Act
            await _walletService.WithdrawFundsAsync(userId, currencySymbol, amountToWithdraw);

            // Assert
            var updatedWallet = await _mockWalletRepository.GetWalletByUserIdAsync(userId, currencySymbol);
            Assert.Equal(450.0, updatedWallet.Balance); // 500 - 50
            _mockWalletRepository.Verify(r => r.SaveWalletAsync(It.IsAny<Wallet>()), Times.Once());
        }

        /// <summary>
        /// Tests that WithdrawFundsAsync throws an InvalidOperationException if the withdrawal amount exceeds the current balance.
        /// </summary>
        [Fact]
        public async Task WithdrawFundsAsync_ThrowsExceptionIfInsufficientBalance()
        {
            // Arrange
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToWithdraw = 600.0; // More than available (500)
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = 500.0 };

            _mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _walletService.WithdrawFundsAsync(userId, currencySymbol, amountToWithdraw));
        }

        /// <summary>
        /// Tests that WithdrawFundsAsync throws an InvalidOperationException if the wallet has no balance set.
        /// </summary>
        [Fact]
        public async Task WithdrawFundsAsync_ThrowsExceptionIfNoBalance()
        {
            // Arrange
            int userId = 2;
            string currencySymbol = "EUR";
            double amountToWithdraw = 10.0;
            var initialWallet = new Wallet { Id = 1, UserId = userId, CurrencySymbol = currencySymbol, Balance = null };

            _mockWalletRepository.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol))
                .ReturnsAsync(initialWallet);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _walletService.WithdrawFundsAsync(userId, currencySymbol, amountToWithdraw));
        }
    }
}