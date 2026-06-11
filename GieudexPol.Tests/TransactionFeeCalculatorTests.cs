using FluentAssertions;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using Moq;

namespace GieudexPol.Tests;

public class TransactionFeeCalculatorTests
{
    private readonly Mock<ICurrencyService> _currencyService = new();
    private readonly Mock<IExchangeRateService> _exchangeRateService = new();
    private readonly Mock<ITransactionFeeRepository> _feeRepository = new();

    [Fact]
    public async Task CalculateAsync_UsesMinimumTenPlnForSmallPlnOperation()
    {
        _currencyService.Setup(service => service.GetByIdAsync(1))
            .ReturnsAsync(new Currency { Id = 1, Symbol = "PLN" });
        var calculator = CreateCalculator();

        var result = await calculator.CalculateAsync("Transfer", 1, 100m);

        result.FeeAmount.Should().Be(10m);
    }

    [Fact]
    public async Task CalculateAsync_UsesHalfPercentWhenItExceedsMinimum()
    {
        _currencyService.Setup(service => service.GetByIdAsync(1))
            .ReturnsAsync(new Currency { Id = 1, Symbol = "PLN" });
        var calculator = CreateCalculator();

        var result = await calculator.CalculateAsync("Withdrawal", 1, 10_000m);

        result.FeeAmount.Should().Be(50m);
    }

    [Fact]
    public async Task CalculateAsync_ConvertsMinimumPlnFeeToOperationCurrency()
    {
        _currencyService.Setup(service => service.GetByIdAsync(2))
            .ReturnsAsync(new Currency { Id = 2, Symbol = "USD" });
        _exchangeRateService.Setup(service => service.GetByCurrencyPairAsync("USD", "PLN"))
            .ReturnsAsync(new ExchangeRate
            {
                BuyPrice = 3.90m,
                SellPrice = 4.10m,
                MidPrice = 4m
            });
        var calculator = CreateCalculator();

        var result = await calculator.CalculateAsync("Deposit", 2, 100m);

        result.FeeAmount.Should().Be(2.5m);
    }

    private TransactionFeeCalculator CreateCalculator()
    {
        return new TransactionFeeCalculator(
            _currencyService.Object,
            _exchangeRateService.Object,
            _feeRepository.Object);
    }
}
