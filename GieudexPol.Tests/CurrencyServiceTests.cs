using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Tests
{
    // Klasa testująca usługę zarządzania walutami (CurrencyService). 
// Przyjęcie struktury, gdzie mocki i usługi są inicjalizowane w każdym teście zapewnia izolację stanów.
public class CurrencyServiceTests
    {
        /// <summary>
        /// Testuje scenariusz pobierania waluty po jej istnieniu (sukces).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ReturnsCurrencyWhenExists()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockCurrencyRepository = new Mock<ICurrencyRepository>();
            var currencyService = new CurrencyService(mockCurrencyRepository.Object);

            var expectedCurrency = new Currency { Id = 1, Symbol = "USD" };
            mockCurrencyRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedCurrency);

            // Act
            var result = await currencyService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("USD", result.Symbol);
            mockCurrencyRepository.Verify(r => r.GetByIdAsync(1), Times.Once());
        }

        /// <summary>
        /// Testuje scenariusz pobierania waluty, która nie istnieje (powinno zwrócić null).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenNotFound()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockCurrencyRepository = new Mock<ICurrencyRepository>();
            var currencyService = new CurrencyService(mockCurrencyRepository.Object);

            mockCurrencyRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Currency)null);

            // Act
            var result = await currencyService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
            mockCurrencyRepository.Verify(r => r.GetByIdAsync(99), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie wszystkich dostępnych walut z systemu.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ReturnsAllCurrencies()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockCurrencyRepository = new Mock<ICurrencyRepository>();
            var currencyService = new CurrencyService(mockCurrencyRepository.Object);

            var currencies = new List<Currency>
            {
                new Currency { Id = 1, Symbol = "USD" },
                new Currency { Id = 2, Symbol = "EUR" }
            };
            mockCurrencyRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(currencies);

            // Act
            var result = await currencyService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            mockCurrencyRepository.Verify(r => r.GetAllAsync(), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa wywołuje odpowiednio metodę dodawania waluty do repozytorium.
        /// </summary>
        [Fact]
        public async Task AddAsync_CallsRepositoryAdd()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockCurrencyRepository = new Mock<ICurrencyRepository>();
            var currencyService = new CurrencyService(mockCurrencyRepository.Object);

            var newCurrency = new Currency { Symbol = "JPY" };

            // Act
            await currencyService.AddAsync(newCurrency);

            // Assert
            mockCurrencyRepository.Verify(r => r.AddAsync(newCurrency), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa wywołuje odpowiednio metodę aktualizacji waluty w repozytorium.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_CallsRepositoryUpdate()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockCurrencyRepository = new Mock<ICurrencyRepository>();
            var currencyService = new CurrencyService(mockCurrencyRepository.Object);

            var updatedCurrency = new Currency { Id = 1, Symbol = "updatedUSD" };

            // Act
            await currencyService.UpdateAsync(updatedCurrency);

            // Assert
            mockCurrencyRepository.Verify(r => r.UpdateAsync(updatedCurrency), Times.Once());
        }

        /// <summary>
        /// Testuje, czy usługa wywołuje odpowiednio metodę usuwania waluty z repozytorium.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_CallsRepositoryDelete()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockCurrencyRepository = new Mock<ICurrencyRepository>();
            var currencyService = new CurrencyService(mockCurrencyRepository.Object);

            var currencyToDelete = new Currency { Id = 1 };

            // Act
            await currencyService.DeleteAsync(currencyToDelete);

            // Assert
            mockCurrencyRepository.Verify(r => r.DeleteAsync(currencyToDelete), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie waluty na podstawie symbolu (sukces).
        /// </summary>
        [Fact]
        public async Task GetBySymbolAsync_ReturnsCurrencyWhenExists()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockCurrencyRepository = new Mock<ICurrencyRepository>();
            var currencyService = new CurrencyService(mockCurrencyRepository.Object);

            var expectedCurrency = new Currency { Id = 2, Symbol = "EUR" };
            mockCurrencyRepository.Setup(r => r.GetBySymbolAsync("eur"))
                .ReturnsAsync(expectedCurrency);

            // Act
            var result = await currencyService.GetBySymbolAsync("eur");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EUR", result.Symbol);
            mockCurrencyRepository.Verify(r => r.GetBySymbolAsync("eur"), Times.Once());
        }

        /// <summary>
        /// Testuje pobranie waluty na podstawie symbolu, który nie istnieje (powinno zwrócić null).
        /// </summary>
        [Fact]
        public async Task GetBySymbolAsync_ReturnsNullWhenNotFound()
        {
            // Arrange: Inicjalizacja dla każdego testu zapewnia izolację stanów.
            var mockCurrencyRepository = new Mock<ICurrencyRepository>();
            var currencyService = new CurrencyService(mockCurrencyRepository.Object);

            mockCurrencyRepository.Setup(r => r.GetBySymbolAsync("nonexistent"))
                .ReturnsAsync((Currency)null);

            // Act
            var result = await currencyService.GetBySymbolAsync("nonexistent");

            // Assert
            Assert.Null(result);
            mockCurrencyRepository.Verify(r => r.GetBySymbolAsync("nonexistent"), Times.Once());
        }
    }
}