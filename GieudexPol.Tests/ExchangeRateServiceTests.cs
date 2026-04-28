using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Tests
{
    public class ExchangeRateServiceTests
    {
        private readonly Mock<IExchangeRateRepository> _mockExchangeRateRepository;
        private readonly ExchangeRateService _exchangeRateService;

        public ExchangeRateServiceTests()
        {
            _mockExchangeRateRepository = new Mock<IExchangeRateRepository>();
            _exchangeRateService = new ExchangeRateService(_mockExchangeRateRepository.Object);
        }

        [Fact]
        public async Task GetByIdAsync_RetrievesCorrectRateById()
        {
            // Arrange
            int id = 1;
            var expectedRate = new ExchangeRate { Id = id, BaseCurrencySymbol = "USD", TargetCurrencySymbol = "EUR", Rate = 0.92 };
            _mockExchangeRateRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(expectedRate);

            // Act
            var result = await _exchangeRateService.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            _mockExchangeRateRepository.Verify(r => r.GetByIdAsync(id), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenNotFound()
        {
            // Arrange
            int id = 99;
            _mockExchangeRateRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ExchangeRate)null);

            // Act
            var result = await _exchangeRateService.GetByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_RetrievesAllAvailableRates()
        {
            // Arrange
            var expectedRates = new List<ExchangeRate>
            {
                new ExchangeRate { Id = 1, BaseCurrencySymbol = "USD", TargetCurrencySymbol = "EUR", Rate = 0.92 },
                new ExchangeRate { Id = 2, BaseCurrencySymbol = "GBP", TargetCurrencySymbol = "JPY", Rate = 185.0 }
            };
            _mockExchangeRateRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedRates);

            // Act
            var result = await _exchangeRateService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockExchangeRateRepository.Verify(r => r.GetAllAsync(), Times.Once());
        }

        [Fact]
        public async Task AddAsync_AddsNewExchangeRateSuccessfully()
        {
            // Arrange
            var newRate = new ExchangeRate { BaseCurrencySymbol = "JPY", TargetCurrencySymbol = "USD", Rate = 0.0067 };

            _mockExchangeRateRepository.Setup(r => r.AddAsync(It.IsAny<ExchangeRate>())).Returns(Task.CompletedTask);

            // Act
            await _exchangeRateService.AddAsync(newRate);

            // Assert
            _mockExchangeRateRepository.Verify(r => r.AddAsync(It.Is<ExchangeRate>(rate => 
                rate.BaseCurrencySymbol == "JPY" && rate.TargetCurrencySymbol == "USD")), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingExchangeRateSuccessfully()
        {
            // Arrange
            var updatedRate = new ExchangeRate { Id = 1, BaseCurrencySymbol = "USD", TargetCurrencySymbol = "EUR", Rate = 0.93 };

            _mockExchangeRateRepository.Setup(r => r.UpdateAsync(It.IsAny<ExchangeRate>())).Returns(Task.CompletedTask);

            // Act
            await _exchangeRateService.UpdateAsync(updatedRate);

            // Assert
            _mockExchangeRateRepository.Verify(r => r.UpdateAsync(It.Is<ExchangeRate>(rate => 
                rate.Id == 1 && rate.BaseCurrencySymbol == "USD" && rate.TargetCurrencySymbol == "EUR")), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_DeletesExchangeRateSuccessfully()
        {
            // Arrange
            var rateToDelete = new ExchangeRate { Id = 2 };

            _mockExchangeRateRepository.Setup(r => r.DeleteAsync(It.IsAny<ExchangeRate>())).Returns(Task.CompletedTask);

            // Act
            await _exchangeRateService.DeleteAsync(rateToDelete);

            // Assert
            _mockExchangeRateRepository.Verify(r => r.DeleteAsync(It.Is<ExchangeRate>(e => e.Id == 2)), Times.Once());
        }

        [Fact]
        public async Task GetByCurrencyPairAsync_RetrievesRateForSpecificPair()
        {
            // Arrange
            string baseSymbol = "USD";
            string targetSymbol = "CAD";
            var expectedRate = new ExchangeRate { Id = 10, BaseCurrencySymbol = baseSymbol, TargetCurrencySymbol = targetSymbol, Rate = 1.35 };
            _mockExchangeRateRepository.Setup(r => r.GetByCurrencyPairAsync(baseSymbol, targetSymbol)).ReturnsAsync(expectedRate);

            // Act
            var result = await _exchangeRateService.GetByCurrencyPairAsync(baseSymbol, targetSymbol);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            _mockExchangeRateRepository.Verify(r => r.GetByCurrencyPairAsync(baseSymbol, targetSymbol), Times.Once());
        }

        [Fact]
        public async Task GetByCurrencyPairAsync_ReturnsNullWhenNoMatchingPairFound()
        {
            // Arrange
            string baseSymbol = "XYZ";
            string targetSymbol = "ABC";
            _mockExchangeRateRepository.Setup(r => r.GetByCurrencyPairAsync(baseSymbol, targetSymbol)).ReturnsAsync((ExchangeRate)null);

            // Act
            var result = await _exchangeRateService.GetByCurrencyPairAsync(baseSymbol, targetSymbol);

            // Assert
            Assert.Null(result);
        }
    }
}