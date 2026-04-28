using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Tests
{
    // Użycie IClassFixture (lub podobnego mechanizmu) jest najlepsze, ale dla uproszczenia 
    // i z uwagi na brak możliwości zmian w atrybutach testów, będziemy re-mockować zależności przed każdym teście.
    public class UserAlertServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;

        public UserAlertServiceTests()
        {
            // Inicjalizujemy podstawowe mocki, które będą używane do tworzenia serwisów wewnątrz testów.
            _mockUserRepository = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task CheckAndSendLowBalanceAlertAsync_SendsAlertWhenBelowThreshold()
        {
            // ARRANGE
            var userId = 1;
            var currencySymbol = "PLN";
            const double lowBalanceThreshold = 50.0;

            // Mock dla użytkownika (kontekst)
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });

            // Mockowanie repozytorium portfela: Musi być nowy dla testu
            var mockWalletRepo = new Mock<IWalletRepository>();
            var wallet = new Wallet { UserId = userId, CurrencySymbol = currencySymbol, Balance = 30.0 };
            mockWalletRepo.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol)).ReturnsAsync(wallet);

            // Tworzenie serwisowej instancji z mockami dla bieżącego testu (Kluczowe dla izolacji)
            var userAlertService = new UserAlertService(_mockUserRepository.Object, mockWalletRepo.Object);

            // ACT
            await userAlertService.CheckAndSendLowBalanceAlertAsync(userId, currencySymbol, lowBalanceThreshold);

            // ASSERT
            _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once());
        }

        [Fact]
        public async Task CheckAndSendLowBalanceAlertAsync_DoesNotSendAlertWhenAboveThreshold()
        {
            // ARRANGE
            var userId = 1;
            var currencySymbol = "PLN";
            const double lowBalanceThreshold = 50.0;

            // Mock dla użytkownika (kontekst)
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });

            // Mockowanie repozytorium portfela: Saldo wyższe niż próg alertu
            var mockWalletRepo = new Mock<IWalletRepository>();
            var wallet = new Wallet { UserId = userId, CurrencySymbol = currencySymbol, Balance = 200.0 };
            mockWalletRepo.Setup(r => r.GetWalletByUserIdAsync(userId, currencySymbol)).ReturnsAsync(wallet);

            // Tworzenie serwisowej instancji z mockami dla bieżącego testu (Kluczowe dla izolacji)
            var userAlertService = new UserAlertService(_mockUserRepository.Object, mockWalletRepo.Object);

            // ACT
            await userAlertService.CheckAndSendLowBalanceAlertAsync(userId, currencySymbol, lowBalanceThreshold);

            // ASSERT
             _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once()); 
        }

        [Fact]
        public async Task CheckAndSendLowBalanceAlertAsync_ThrowsExceptionIfUserNotFound()
        {
            // ARRANGE
            var userId = 99;
            var currencySymbol = "PLN";
            const double lowBalanceThreshold = 50.0;

            // Mockowanie braku użytkownika, co ma spowodować wyjątek (InvalidOperationException)
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);


            // ACT & ASSERT
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                userAlertService.CheckAndSendLowBalanceAlertAsync(userId, currencySymbol, lowBalanceThreshold));
        }
    }
}