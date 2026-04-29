/// <summary>
/// Klasa testująca usługę kursów wymiany walut (ExchangeRateService).
/// Testy te weryfikują logikę zarządzania, wyszukiwania i aktualizacji starych danych o parach walutowych.
/// </summary>
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Tests
{
    // Klasa testująca usługę zarządzania kursami wymiany walut (ExchangeRateService). 
// Przyjęcie struktury, gdzie mocki i usługi są inicjalizowane w każdym teście zapewnia izolację stanów.
public class ExchangeRateServiceTests
    {
        /// <summary>
        /// Testuje pobieranie konkretnego kursu wymiany po jego ID (sukces).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_RetrievesCorrectRateById()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            var exchangeRateService = new ExchangeRateService(mockExchangeRateRepository.Object);

            int id = 1;
            var expectedRate = new ExchangeRate { Id = id, BaseCurrencySymbol = "USD", TargetCurrencySymbol = "EUR", Rate = 0.92 };
            mockExchangeRateRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(expectedRate);

            // Act
            var result = await exchangeRateService.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            mockExchangeRateRepository.Verify(r => r.GetByIdAsync(id), Times.Once());
        }

        /// <summary>
        /// Testuje pobieranie kursu wymiany, który nie istnieje (powinno zwrócić null).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenNotFound()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            var exchangeRateService = new ExchangeRateService(mockExchangeRateRepository.Object);

            int id = 99;
            mockExchangeRateRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ExchangeRate)null);

            // Act
            var result = await exchangeRateService.GetByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Testuje pobranie wszystkich dostępnych kursów wymiany.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_RetrievesAllAvailableRates()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            var exchangeRateService = new ExchangeRateService(mockExchangeRateRepository.Object);

            var expectedRates = new List<ExchangeRate>
            {
                new ExchangeRate { Id = 1, BaseCurrencySymbol = "USD", TargetCurrencySymbol = "EUR", Rate = 0.92 },
                new ExchangeRate { Id = 2, BaseCurrencySymbol = "GBP", TargetCurrencySymbol = "JPY", Rate = 185.0 }
            };
            mockExchangeRateRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedRates);

            // Act
            var result = await exchangeRateService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            mockExchangeRateRepository.Verify(r => r.GetAllAsync(), Times.Once());
        }

        /// <summary>
        /// Testuje dodawanie nowego kursu wymiany do repozytorium.
        /// </summary>
        [Fact]
        public async Task AddAsync_AddsNewExchangeRateSuccessfully()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            var exchangeRateService = new ExchangeRateService(mockExchangeRateRepository.Object);

            var newRate = new ExchangeRate { BaseCurrencySymbol = "JPY", TargetCurrencySymbol = "USD", Rate = 0.0067 };

            mockExchangeRateRepository.Setup(r => r.AddAsync(It.IsAny<ExchangeRate>())).Returns(Task.CompletedTask);

            // Act
            await exchangeRateService.AddAsync(newRate);

            // Assert
            mockExchangeRateRepository.Verify(r => r.AddAsync(It.Is<ExchangeRate>(rate => 
                rate.BaseCurrencySymbol == "JPY" && rate.TargetCurrencySymbol == "USD")), Times.Once());
        }

        /// <summary>
        /// Testuje aktualizację istniejącego kursu wymiany w repozytorium.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_UpdatesExistingExchangeRateSuccessfully()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            var exchangeRateService = new ExchangeRateService(mockExchangeRateRepository.Object);

            var updatedRate = new ExchangeRate { Id = 1, BaseCurrencySymbol = "USD", TargetCurrencySymbol = "EUR", Rate = 0.93 };

            mockExchangeRateRepository.Setup(r => r.UpdateAsync(It.IsAny<ExchangeRate>())).Returns(Task.CompletedTask);

            // Act
            await exchangeRateService.UpdateAsync(updatedRate);

            // Assert
            mockExchangeRateRepository.Verify(r => r.UpdateAsync(It.Is<ExchangeRate>(rate => 
                rate.Id == 1 && rate.BaseCurrencySymbol == "USD" && rate.TargetCurrencySymbol == "EUR")), Times.Once());
        }

        /// <summary>
        /// Testuje usunięcie kursu wymiany z repozytorium.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_DeletesExchangeRateSuccessfully()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            var exchangeRateService = new ExchangeRateService(mockExchangeRateRepository.Object);

            var rateToDelete = new ExchangeRate { Id = 2 };

            mockExchangeRateRepository.Setup(r => r.DeleteAsync(It.IsAny<ExchangeRate>())).Returns(Task.CompletedTask);

            // Act
            await exchangeRateService.DeleteAsync(rateToDelete);

            // Assert
            mockExchangeRateRepository.Verify(r => r.DeleteAsync(It.Is<ExchangeRate>(e => e.Id == 2)), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie kursu wymiany dla konkretnej pary walut (sukces).
        /// </summary>
        [Fact]
        public async Task GetByCurrencyPairAsync_RetrievesRateForSpecificPair()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            var exchangeRateService = new ExchangeRateService(mockExchangeRateRepository.Object);

            string baseSymbol = "USD";
            string targetSymbol = "CAD";
            var expectedRate = new ExchangeRate { Id = 10, BaseCurrencySymbol = baseSymbol, TargetCurrencySymbol = targetSymbol, Rate = 1.35 };
            mockExchangeRateRepository.Setup(r => r.GetByCurrencyPairAsync(baseSymbol, targetSymbol)).ReturnsAsync(expectedRate);

            // Act
            var result = await exchangeRateService.GetByCurrencyPairAsync(baseSymbol, targetSymbol);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            mockExchangeRateRepository.Verify(r => r.GetByCurrencyPairAsync(baseSymbol, targetSymbol), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie kursu dla pary walut, która nie istnieje (powinno zwrócić null).
        /// </summary>
        [Fact]
        public async Task GetByCurrencyPairAsync_ReturnsNullWhenNoMatchingPairFound()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            var exchangeRateService = new ExchangeRateService(mockExchangeRateRepository.Object);

            string baseSymbol = "XYZ";
            string targetSymbol = "ABC";
            mockExchangeRateRepository.Setup(r => r.GetByCurrencyPairAsync(baseSymbol, targetSymbol)).ReturnsAsync((ExchangeRate)null);

            // Act
            var result = await exchangeRateService.GetByCurrencyPairAsync(baseSymbol, targetSymbol);

            // Assert
            Assert.Null(result);
        }
    }
}