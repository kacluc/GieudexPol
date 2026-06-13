using FluentAssertions;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class UserTradingAlertServiceTests
{
    [Fact]
    public async Task WantToBuy_RejectsDirectionOtherThanBelowOrEqual()
    {
        await using var context = await CreateContextAsync();
        var alert = CreateAlert(
            TradingAlertEvent.SellOrder,
            ThresholdDirection.AboveOrEqual);

        var action = () => new UserTradingAlertService(context).CreateAsync(alert);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*<=*");
    }

    [Fact]
    public async Task WantToSell_RejectsDirectionOtherThanAboveOrEqual()
    {
        await using var context = await CreateContextAsync();
        var alert = CreateAlert(
            TradingAlertEvent.BuyOrder,
            ThresholdDirection.BelowOrEqual);

        var action = () => new UserTradingAlertService(context).CreateAsync(alert);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*>=*");
    }

    private static async Task<ApplicationDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.AddRange(
            new Currency { Id = 1, Symbol = "EUR", Name = "Euro", IsActive = true },
            new Currency { Id = 2, Symbol = "PLN", Name = "Polski zloty", IsActive = true });
        context.TradingPairs.Add(new TradingPair
        {
            Id = 1,
            BaseCurrencyId = 1,
            QuoteCurrencyId = 2,
            IsActive = true,
            TickSize = 0.0001m
        });
        await context.SaveChangesAsync();
        return context;
    }

    private static UserTradingAlert CreateAlert(
        TradingAlertEvent eventType,
        ThresholdDirection direction)
    {
        return new UserTradingAlert
        {
            UserId = 1,
            TradingPairId = 1,
            EventType = eventType,
            Direction = direction,
            TargetPrice = 4.30m
        };
    }
}
