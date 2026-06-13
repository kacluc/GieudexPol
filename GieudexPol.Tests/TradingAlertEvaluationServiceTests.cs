using FluentAssertions;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class TradingAlertEvaluationServiceTests
{
    [Fact]
    public async Task BuyOrderAlert_UsesHighestActiveBuyPriceFromOtherUsers()
    {
        await using var context = CreateContext();
        var seed = await SeedMarketAsync(context);
        context.Orders.AddRange(
            CreateOrder(seed.Pair, seed.OtherUser, OrderSide.Buy, 4.30m, 25m),
            CreateOrder(seed.Pair, seed.OtherUser, OrderSide.Buy, 4.35m, 10m),
            CreateOrder(seed.Pair, seed.AlertOwner, OrderSide.Buy, 5.00m, 100m));
        context.UserTradingAlerts.Add(CreateAlert(
            seed,
            TradingAlertEvent.BuyOrder,
            ThresholdDirection.AboveOrEqual,
            4.34m));
        await context.SaveChangesAsync();

        var result = await new TradingAlertEvaluationService(context)
            .EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("4.35");
        context.UserTradingAlerts.Single().Status.Should().Be(AlertStatus.Fulfilled);
        context.AlertLogs.Single().CurrentPrice.Should().Be(4.35m);
    }

    [Fact]
    public async Task SellOrderAlert_UsesLowestActiveSellPriceAndAggregatedLevelAmount()
    {
        await using var context = CreateContext();
        var seed = await SeedMarketAsync(context);
        context.Orders.AddRange(
            CreateOrder(seed.Pair, seed.OtherUser, OrderSide.Sell, 4.25m, 6m),
            CreateOrder(seed.Pair, seed.ThirdUser, OrderSide.Sell, 4.25m, 5m),
            CreateOrder(seed.Pair, seed.OtherUser, OrderSide.Sell, 4.30m, 50m));
        context.UserTradingAlerts.Add(CreateAlert(
            seed,
            TradingAlertEvent.SellOrder,
            ThresholdDirection.BelowOrEqual,
            4.26m,
            minimumAmount: 10m));
        await context.SaveChangesAsync();

        var result = await new TradingAlertEvaluationService(context)
            .EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("ilosc: 11");
    }

    [Fact]
    public async Task WantToBuy_UsesLowestSellAndBelowOrEqualEvenForLegacyDirection()
    {
        await using var context = CreateContext();
        var seed = await SeedMarketAsync(context);
        context.Orders.AddRange(
            CreateOrder(seed.Pair, seed.OtherUser, OrderSide.Sell, 4.20m, 5m),
            CreateOrder(seed.Pair, seed.ThirdUser, OrderSide.Sell, 4.30m, 5m));
        context.UserTradingAlerts.Add(CreateAlert(
            seed,
            TradingAlertEvent.SellOrder,
            ThresholdDirection.AboveOrEqual,
            4.25m));
        await context.SaveChangesAsync();

        var result = await new TradingAlertEvaluationService(context)
            .EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("4.2");
    }

    [Fact]
    public async Task WantToSell_UsesHighestBuyAndAboveOrEqualEvenForLegacyDirection()
    {
        await using var context = CreateContext();
        var seed = await SeedMarketAsync(context);
        context.Orders.AddRange(
            CreateOrder(seed.Pair, seed.OtherUser, OrderSide.Buy, 4.40m, 5m),
            CreateOrder(seed.Pair, seed.ThirdUser, OrderSide.Buy, 4.30m, 5m));
        context.UserTradingAlerts.Add(CreateAlert(
            seed,
            TradingAlertEvent.BuyOrder,
            ThresholdDirection.BelowOrEqual,
            4.35m));
        await context.SaveChangesAsync();

        var result = await new TradingAlertEvaluationService(context)
            .EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("4.4");
    }

    [Fact]
    public async Task TradeExecutionAlert_TriggersOnlyForExecutionCreatedAfterAlert()
    {
        await using var context = CreateContext();
        var seed = await SeedMarketAsync(context);
        var buy = CreateOrder(seed.Pair, seed.OtherUser, OrderSide.Buy, 4.40m, 5m);
        var sell = CreateOrder(seed.Pair, seed.ThirdUser, OrderSide.Sell, 4.40m, 5m);
        context.Orders.AddRange(buy, sell);
        var alert = CreateAlert(
            seed,
            TradingAlertEvent.TradeExecution,
            ThresholdDirection.AboveOrEqual,
            4.35m);
        alert.CreatedDate = DateTime.UtcNow.AddMinutes(-5);
        context.UserTradingAlerts.Add(alert);
        await context.SaveChangesAsync();
        context.TradeExecutions.AddRange(
            CreateExecution(seed.Pair, buy, sell, 4.50m, DateTime.UtcNow.AddMinutes(-10)),
            CreateExecution(seed.Pair, buy, sell, 4.40m, DateTime.UtcNow.AddMinutes(-1)));
        await context.SaveChangesAsync();

        var result = await new TradingAlertEvaluationService(context)
            .EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("wykonana transakcja");
        context.Notifications.Single().Message.Should().Contain("4.4");
    }

    [Fact]
    public async Task TradeExecutionAlert_RemainsActiveWhenOnlyHistoricalExecutionMatches()
    {
        await using var context = CreateContext();
        var seed = await SeedMarketAsync(context);
        var buy = CreateOrder(seed.Pair, seed.OtherUser, OrderSide.Buy, 4.50m, 5m);
        var sell = CreateOrder(seed.Pair, seed.ThirdUser, OrderSide.Sell, 4.50m, 5m);
        context.Orders.AddRange(buy, sell);
        var alert = CreateAlert(
            seed,
            TradingAlertEvent.TradeExecution,
            ThresholdDirection.AboveOrEqual,
            4.40m);
        alert.CreatedDate = DateTime.UtcNow;
        context.UserTradingAlerts.Add(alert);
        await context.SaveChangesAsync();
        context.TradeExecutions.Add(
            CreateExecution(seed.Pair, buy, sell, 4.50m, alert.CreatedDate.AddSeconds(-1)));
        await context.SaveChangesAsync();

        var result = await new TradingAlertEvaluationService(context)
            .EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(0);
        context.Notifications.Should().BeEmpty();
        context.UserTradingAlerts.Single().Status.Should().Be(AlertStatus.Active);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<MarketSeed> SeedMarketAsync(ApplicationDbContext context)
    {
        var eur = new Currency { Id = 1, Symbol = "EUR", Name = "Euro" };
        var pln = new Currency { Id = 2, Symbol = "PLN", Name = "Polski zloty" };
        var owner = new User { Id = 1, Username = "owner@example.com" };
        var other = new User { Id = 2, Username = "other@example.com" };
        var third = new User { Id = 3, Username = "third@example.com" };
        var pair = new TradingPair
        {
            Id = 1,
            BaseCurrency = eur,
            QuoteCurrency = pln,
            IsActive = true,
            TickSize = 0.0001m
        };
        context.AddRange(owner, other, third, pair);
        await context.SaveChangesAsync();
        return new MarketSeed(owner, other, third, pair);
    }

    private static UserTradingAlert CreateAlert(
        MarketSeed seed,
        TradingAlertEvent eventType,
        ThresholdDirection direction,
        decimal price,
        decimal? minimumAmount = null)
    {
        return new UserTradingAlert
        {
            User = seed.AlertOwner,
            TradingPair = seed.Pair,
            EventType = eventType,
            Direction = direction,
            TargetPrice = price,
            MinimumAmount = minimumAmount,
            Status = AlertStatus.Active,
            CreatedDate = DateTime.UtcNow.AddMinutes(-1)
        };
    }

    private static Order CreateOrder(
        TradingPair pair,
        User user,
        OrderSide side,
        decimal price,
        decimal amount)
    {
        return new Order
        {
            User = user,
            TradingPair = pair,
            Side = side,
            Type = OrderType.Limit,
            Status = OrderStatus.Open,
            Price = price,
            OriginalAmount = amount,
            RemainingAmount = amount,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static TradeExecution CreateExecution(
        TradingPair pair,
        Order buy,
        Order sell,
        decimal price,
        DateTime executedAt)
    {
        return new TradeExecution
        {
            TradingPair = pair,
            BuyOrder = buy,
            SellOrder = sell,
            Price = price,
            Amount = 5m,
            ExecutedAt = executedAt
        };
    }

    private sealed record MarketSeed(
        User AlertOwner,
        User OtherUser,
        User ThirdUser,
        TradingPair Pair);
}
