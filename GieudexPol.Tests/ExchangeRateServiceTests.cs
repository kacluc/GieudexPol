using Xunit;
using Moq;
using FluentAssertions;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public async Task AddAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var exchangeRate = new ExchangeRate { CurrencyId = 1, BuyPrice = 1.2m, SellPrice = 1.3m };
            _mockExchangeRateRepository.Setup(repo => repo.AddAsync(exchangeRate)).Returns(Task.CompletedTask);

            // Act
            await _exchangeRateService.AddAsync(exchangeRate);

            // Assert
            _mockExchangeRateRepository.Verify(repo => repo.AddAsync(exchangeRate), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnExchangeRate_WhenExchangeRateExists()
        {
            // Arrange
            var exchangeRateId = 1;
            var expectedExchangeRate = new ExchangeRate { Id = exchangeRateId, CurrencyId = 1, BuyPrice = 1.2m, SellPrice = 1.3m };
            _mockExchangeRateRepository.Setup(repo => repo.GetByIdAsync(exchangeRateId)).ReturnsAsync(expectedExchangeRate);

            // Act
            var result = await _exchangeRateService.GetByIdAsync(exchangeRateId);

            // Assert
            result.Should().BeEquivalentTo(expectedExchangeRate);
            _mockExchangeRateRepository.Verify(repo => repo.GetByIdAsync(exchangeRateId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenExchangeRateDoesNotExist()
        {
            // Arrange
            var exchangeRateId = 1;
            _mockExchangeRateRepository.Setup(repo => repo.GetByIdAsync(exchangeRateId)).ReturnsAsync((ExchangeRate)null);

            // Act
            var result = await _exchangeRateService.GetByIdAsync(exchangeRateId);

            // Assert
            result.Should().BeNull();
            _mockExchangeRateRepository.Verify(repo => repo.GetByIdAsync(exchangeRateId), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllExchangeRates()
        {
            // Arrange
            var expectedExchangeRates = new List<ExchangeRate>
            {
                new ExchangeRate { Id = 1, CurrencyId = 1, BuyPrice = 1.2m, SellPrice = 1.3m },
                new ExchangeRate { Id = 2, CurrencyId = 2, BuyPrice = 0.8m, SellPrice = 0.9m }
            };
            _mockExchangeRateRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedExchangeRates);

            // Act
            var result = await _exchangeRateService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedExchangeRates);
            _mockExchangeRateRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
        {
            // Arrange
            var exchangeRate = new ExchangeRate { Id = 1, CurrencyId = 1, BuyPrice = 1.3m, SellPrice = 1.4m };
            _mockExchangeRateRepository.Setup(repo => repo.UpdateAsync(exchangeRate)).Returns(Task.CompletedTask);

            // Act
            await _exchangeRateService.UpdateAsync(exchangeRate);

            // Assert
            _mockExchangeRateRepository.Verify(repo => repo.UpdateAsync(exchangeRate), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var exchangeRate = new ExchangeRate { Id = 1, CurrencyId = 1, BuyPrice = 1.2m, SellPrice = 1.3m };
            _mockExchangeRateRepository.Setup(repo => repo.DeleteAsync(exchangeRate)).Returns(Task.CompletedTask);

            // Act
            await _exchangeRateService.DeleteAsync(exchangeRate);

            // Assert
            _mockExchangeRateRepository.Verify(repo => repo.DeleteAsync(exchangeRate), Times.Once);
        }
    }
}
