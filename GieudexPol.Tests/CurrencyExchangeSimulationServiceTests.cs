using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using Moq;

namespace GieudexPol.Tests;

public class CurrencyExchangeSimulationServiceTests
{
    [Fact]
    public async Task SimulateExchangeAsync_ShouldUseSellPriceWhenUserBuysCurrency()
    {
        var exchangeRateService = new Mock<IExchangeRateService>();
        exchangeRateService
            .Setup(service => service.GetByCurrencyPairAsync("EUR", "PLN"))
            .ReturnsAsync(new ExchangeRate { BuyPrice = 4.90m, SellPrice = 5.10m });
        var service = new CurrencyExchangeSimulationService(exchangeRateService.Object);

        var result = await service.SimulateExchangeAsync(new CurrencyExchangeSimulationRequestDto
        {
            Amount = 102m,
            SourceCurrency = "PLN",
            TargetCurrency = "EUR"
        });

        result.ExchangedAmount.Should().Be(20m);
        result.ExchangeRate.Should().Be(5.10m);
    }

    [Fact]
    public async Task SimulateExchangeAsync_ShouldUseBuyPriceWhenUserSellsCurrency()
    {
        var exchangeRateService = new Mock<IExchangeRateService>();
        exchangeRateService
            .Setup(service => service.GetByCurrencyPairAsync("EUR", "PLN"))
            .ReturnsAsync(new ExchangeRate { BuyPrice = 4.90m, SellPrice = 5.10m });
        var service = new CurrencyExchangeSimulationService(exchangeRateService.Object);

        var result = await service.SimulateExchangeAsync(new CurrencyExchangeSimulationRequestDto
        {
            Amount = 20m,
            SourceCurrency = "EUR",
            TargetCurrency = "PLN"
        });

        result.ExchangedAmount.Should().Be(98m);
        result.ExchangeRate.Should().Be(4.90m);
    }
}
