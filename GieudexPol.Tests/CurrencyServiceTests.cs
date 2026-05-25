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
        public async Task AddAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var currency = new Currency { Name = "Test Currency", Symbol = "TC" };
            _mockCurrencyRepository.Setup(repo => repo.AddAsync(currency)).Returns(Task.CompletedTask);

            // Act
            await _currencyService.AddAsync(currency);

            // Assert
            _mockCurrencyRepository.Verify(repo => repo.AddAsync(currency), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCurrency_WhenCurrencyExists()
        {
            // Arrange
            var currencyId = 1;
            var expectedCurrency = new Currency { Id = currencyId, Name = "Test Currency", Symbol = "TC" };
            _mockCurrencyRepository.Setup(repo => repo.GetByIdAsync(currencyId)).ReturnsAsync(expectedCurrency);

            // Act
            var result = await _currencyService.GetByIdAsync(currencyId);

            // Assert
            result.Should().BeEquivalentTo(expectedCurrency);
            _mockCurrencyRepository.Verify(repo => repo.GetByIdAsync(currencyId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCurrencyDoesNotExist()
        {
            // Arrange
            var currencyId = 1;
            _mockCurrencyRepository.Setup(repo => repo.GetByIdAsync(currencyId)).ReturnsAsync((Currency?)null);

            // Act
            var result = await _currencyService.GetByIdAsync(currencyId);

            // Assert
            result.Should().BeNull();
            _mockCurrencyRepository.Verify(repo => repo.GetByIdAsync(currencyId), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCurrencies()
        {
            // Arrange
            var expectedCurrencies = new List<Currency>
            {
                new Currency { Id = 1, Name = "Currency 1", Symbol = "C1" },
                new Currency { Id = 2, Name = "Currency 2", Symbol = "C2" }
            };
            _mockCurrencyRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedCurrencies);

            // Act
            var result = await _currencyService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedCurrencies);
            _mockCurrencyRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
        {
            // Arrange
            var currency = new Currency { Id = 1, Name = "Updated Currency", Symbol = "UC" };
            _mockCurrencyRepository.Setup(repo => repo.UpdateAsync(currency)).Returns(Task.CompletedTask);

            // Act
            await _currencyService.UpdateAsync(currency);

            // Assert
            _mockCurrencyRepository.Verify(repo => repo.UpdateAsync(currency), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var currency = new Currency { Id = 1, Name = "Test Currency", Symbol = "TC" };
            _mockCurrencyRepository.Setup(repo => repo.DeleteAsync(currency)).Returns(Task.CompletedTask);

            // Act
            await _currencyService.DeleteAsync(currency);

            // Assert
            _mockCurrencyRepository.Verify(repo => repo.DeleteAsync(currency), Times.Once);
        }
    }
}
