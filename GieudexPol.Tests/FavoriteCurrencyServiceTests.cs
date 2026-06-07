using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;

namespace GieudexPol.Tests
{
    public class FavoriteCurrencyServiceTests
    {
        private readonly Mock<IFavoriteCurrencyRepository> _repository = new();
        private readonly FavoriteCurrencyService _service;

        public FavoriteCurrencyServiceTests()
        {
            _service = new FavoriteCurrencyService(_repository.Object);
        }

        [Fact]
        public async Task GetFavoritesAsync_ShouldReturnCurrencyCodes()
        {
            _repository.Setup(repository => repository.GetFavoritesAsync())
                .ReturnsAsync(
                [
                    new FavoriteCurrency { CurrencyCode = "EUR" },
                    new FavoriteCurrency { CurrencyCode = "USD" }
                ]);

            var result = await _service.GetFavoritesAsync();

            result.Select(item => item.CurrencyCode).Should().Equal("EUR", "USD");
        }

        [Fact]
        public async Task AddFavoriteAsync_ShouldNormalizeAndPersistSupportedCurrency()
        {
            _repository.Setup(repository => repository.ExistsAsync("EUR")).ReturnsAsync(false);

            await _service.AddFavoriteAsync(new AddFavoriteCurrencyDto { CurrencyCode = " eur " });

            _repository.Verify(repository => repository.AddAsync(
                It.Is<FavoriteCurrency>(currency => currency.CurrencyCode == "EUR")), Times.Once);
        }

        [Fact]
        public async Task AddFavoriteAsync_ShouldRejectUnsupportedCurrency()
        {
            var action = () => _service.AddFavoriteAsync(
                new AddFavoriteCurrencyDto { CurrencyCode = "XYZ" });

            await action.Should().ThrowAsync<InvalidOperationException>();
            _repository.Verify(repository => repository.AddAsync(It.IsAny<FavoriteCurrency>()), Times.Never);
        }
    }
}

