using FluentAssertions;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Repositories;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class AlertDeletionTests
{
    [Fact]
    public async Task DeletingRateAlert_DeletesItsLogs()
    {
        await using var context = CreateContext();
        var currency = new Currency
        {
            Id = 1,
            Symbol = "EUR",
            Name = "Euro",
            IsActive = true
        };
        var alert = new UserAlert
        {
            UserId = 1,
            Currency = currency,
            AlertType = AlertType.Threshold,
            PriceSide = AlertPriceSide.UserSellsCurrency,
            ThresholdDirection = ThresholdDirection.AboveOrEqual,
            ThresholdValue = 4.50m,
            Status = AlertStatus.Fulfilled,
            CreatedDate = DateTime.UtcNow
        };
        context.UserAlerts.Add(alert);
        context.AlertLogs.Add(new AlertLog
        {
            UserAlert = alert,
            Message = "Warunek spelniony.",
            CreatedDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        await new UserAlertRepository(context).DeleteAsync(alert);

        context.UserAlerts.Should().BeEmpty();
        context.AlertLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task DeletingTradingAlert_DeletesItsLogs()
    {
        await using var context = CreateContext();
        var baseCurrency = new Currency
        {
            Id = 1,
            Symbol = "EUR",
            Name = "Euro",
            IsActive = true
        };
        var quoteCurrency = new Currency
        {
            Id = 2,
            Symbol = "PLN",
            Name = "Polski zloty",
            IsActive = true
        };
        var pair = new TradingPair
        {
            Id = 1,
            BaseCurrency = baseCurrency,
            QuoteCurrency = quoteCurrency,
            IsActive = true,
            TickSize = 0.0001m
        };
        var alert = new UserTradingAlert
        {
            UserId = 1,
            TradingPair = pair,
            EventType = TradingAlertEvent.SellOrder,
            Direction = ThresholdDirection.BelowOrEqual,
            TargetPrice = 4.30m,
            Status = AlertStatus.Fulfilled,
            CreatedDate = DateTime.UtcNow
        };
        context.UserTradingAlerts.Add(alert);
        context.AlertLogs.Add(new AlertLog
        {
            UserTradingAlert = alert,
            Message = "Oferta spelnia warunek.",
            CreatedDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        await new UserTradingAlertService(context).DeleteAsync(alert);

        context.UserTradingAlerts.Should().BeEmpty();
        context.AlertLogs.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
