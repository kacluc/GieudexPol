using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Tests
{
    public class CurrencyServiceTests
    {
        private readonly Mock<ICurrencyRepository> _mockCurrencyRepository;
        private readonly CurrencyService _currencyService;

        public CurrencyServiceTests()
        {
            _mockCurrencyRepository = new Mock<ICurrencyRepository>();
            _currencyService = new CurrencyService(_mockCurrencyRepository.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCurrencyWhenExists()
        {
            // Arrange
            var expectedCurrency = new Currency { Id = 1, Symbol = "USD" };
            _mockCurrencyRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedCurrency);

            // Act
            var result = await _currencyService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("USD", result.Symbol);
            _mockCurrencyRepository.Verify(r => r.GetByIdAsync(1), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenNotFound()
        {
            // Arrange
            _mockCurrencyRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Currency)null);

            // Act
            var result = await _currencyService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
            _mockCurrencyRepository.Verify(r => r.GetByIdAsync(99), Times.Once());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCurrencies()
        {
            // Arrange
            var currencies = new List<Currency>
            {
                new Currency { Id = 1, Symbol = "USD" },
                new Currency { Id = 2, Symbol = "EUR" }
            };
            _mockCurrencyRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(currencies);

            // Act
            var result = await _currencyService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCurrencyRepository.Verify(r => r.GetAllAsync(), Times.Once());
        }

        [Fact]
        public async Task AddAsync_CallsRepositoryAdd()
        {
            // Arrange
            var newCurrency = new Currency { Symbol = "JPY" };

            // Act
            await _currencyService.AddAsync(newCurrency);

            // Assert
            _mockCurrencyRepository.Verify(r => r.AddAsync(newCurrency), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_CallsRepositoryUpdate()
        {
            // Arrange
            var updatedCurrency = new Currency { Id = 1, Symbol = "updatedUSD" };

            // Act
            await _currencyService.UpdateAsync(updatedCurrency);

            // Assert
            _mockCurrencyRepository.Verify(r => r.UpdateAsync(updatedCurrency), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_CallsRepositoryDelete()
        {
            // Arrange
            var currencyToDelete = new Currency { Id = 1 };

            // Act
            await _currencyService.DeleteAsync(currencyToDelete);

            // Assert
            _mockCurrencyRepository.Verify(r => r.DeleteAsync(currencyToDelete), Times.Once());
        }

        [Fact]
        public async Task GetBySymbolAsync_ReturnsCurrencyWhenExists()
        {
            // Arrange
            var expectedCurrency = new Currency { Id = 2, Symbol = "EUR" };
            _mockCurrencyRepository.Setup(r => r.GetBySymbolAsync("eur"))
                .ReturnsAsync(expectedCurrency);

            // Act
            var result = await _currencyService.GetBySymbolAsync("eur");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EUR", result.Symbol);
            _mockCurrencyRepository.Verify(r => r.GetBySymbolAsync("eur"), Times.Once());
        }

        [Fact]
        public async Task GetBySymbolAsync_ReturnsNullWhenNotFound()
        {
            // Arrange
            _mockCurrencyRepository.Setup(r => r.GetBySymbolAsync("nonexistent"))
                .ReturnsAsync((Currency)null);

            // Act
            var result = await _currencyService.GetBySymbolAsync("nonexistent");

            // Assert
            Assert.Null(result);
            _mockCurrencyRepository.Verify(r => r.GetBySymbolAsync("nonexistent"), Times.Once());
        }
    }
}