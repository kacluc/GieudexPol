using FluentAssertions;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class AlertEvaluationServiceTests
{
    [Fact]
    public async Task ThresholdAboveOrEqual_ForUserSelling_UsesBuyPrice()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.60m, 4.70m, new DateTime(2026, 6, 11));
        AddAlert(context, data, AlertType.Threshold, AlertPriceSide.UserSellsCurrency,
            threshold: 4.50m, direction: ThresholdDirection.AboveOrEqual);
        await context.SaveChangesAsync();

        var result = await CreateService(context).EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("MOCK_BANK_A");
        context.Notifications.Single().Message.Should().Contain("4.6");
    }

    [Fact]
    public async Task ThresholdBelowOrEqual_ForUserBuying_UsesSellPrice()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.00m, 4.22m, new DateTime(2026, 6, 11));
        AddAlert(context, data, AlertType.Threshold, AlertPriceSide.UserBuysCurrency,
            threshold: 4.25m, direction: ThresholdDirection.BelowOrEqual);
        await context.SaveChangesAsync();

        var result = await CreateService(context).EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("4.22");
    }

    [Theory]
    [InlineData(null, 4.3)]
    [InlineData(4.4, 4.4)]
    public async Task Threshold_ForMidPrice_UsesMidOrSpreadAverage(
        double? midPrice,
        double expectedPrice)
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(
            context,
            data,
            data.SourceA,
            4.20m,
            4.40m,
            new DateTime(2026, 6, 11),
            midPrice.HasValue ? (decimal)midPrice.Value : null);
        AddAlert(context, data, AlertType.Threshold, AlertPriceSide.MidPrice,
            threshold: (decimal)expectedPrice, direction: ThresholdDirection.AboveOrEqual);
        await context.SaveChangesAsync();

        var result = await CreateService(context).EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
    }

    [Fact]
    public async Task PriceIncrease_ComparesWithPreviousAvailableRate()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.00m, 4.10m, new DateTime(2026, 6, 10));
        AddRate(context, data, data.SourceA, 4.12m, 4.25m, new DateTime(2026, 6, 11));
        AddAlert(context, data, AlertType.PriceIncrease, AlertPriceSide.UserSellsCurrency,
            percentage: 3m);
        await context.SaveChangesAsync();

        var result = await CreateService(context).EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("+3");
    }

    [Fact]
    public async Task PriceDrop_ComparesWithPreviousAvailableRate()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.00m, 4.20m, new DateTime(2026, 6, 10));
        AddRate(context, data, data.SourceA, 3.90m, 4.05m, new DateTime(2026, 6, 11));
        AddAlert(context, data, AlertType.PriceDrop, AlertPriceSide.UserBuysCurrency,
            percentage: 3m);
        await context.SaveChangesAsync();

        var result = await CreateService(context).EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        context.Notifications.Single().Message.Should().Contain("-");
    }

    [Fact]
    public async Task AlertWithoutSource_EvaluatesManySourcesAndCreatesOneNotification()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.60m, 4.70m, new DateTime(2026, 6, 11));
        AddRate(context, data, data.SourceB, 4.70m, 4.80m, new DateTime(2026, 6, 11));
        var alert = AddAlert(
            context,
            data,
            AlertType.Threshold,
            AlertPriceSide.UserSellsCurrency,
            threshold: 4.50m,
            direction: ThresholdDirection.AboveOrEqual);
        await context.SaveChangesAsync();

        var result = await CreateService(context).EvaluateAllActiveAlertsAsync();

        result.TriggeredAlertsCount.Should().Be(1);
        result.NotificationsCreatedCount.Should().Be(1);
        context.Notifications.Should().ContainSingle();
        context.Notifications.Single().Message.Should().Contain("MOCK_BANK_A");
        context.Notifications.Single().Message.Should().Contain("MOCK_BANK_B");
        alert.Status.Should().Be(AlertStatus.Fulfilled);
        alert.TriggeredDate.Should().NotBeNull();
        context.AlertLogs.Should().ContainSingle(log =>
            log.UserAlertId == alert.Id &&
            log.SourceSummary != null &&
            log.SourceSummary.Contains("MOCK_BANK_A") &&
            log.SourceSummary.Contains("MOCK_BANK_B"));
    }

    [Fact]
    public async Task AlertWithSpecificSource_EvaluatesOnlyThatSource()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.60m, 4.70m, new DateTime(2026, 6, 11));
        AddRate(context, data, data.SourceB, 4.70m, 4.80m, new DateTime(2026, 6, 11));
        AddAlert(
            context,
            data,
            AlertType.Threshold,
            AlertPriceSide.UserSellsCurrency,
            threshold: 4.50m,
            direction: ThresholdDirection.AboveOrEqual,
            sourceId: data.SourceB.Id);
        await context.SaveChangesAsync();

        await CreateService(context).EvaluateAllActiveAlertsAsync();

        var message = context.Notifications.Single().Message;
        message.Should().Contain("MOCK_BANK_B");
        message.Should().NotContain("MOCK_BANK_A");
    }

    [Fact]
    public async Task SameEffectiveDate_IsNotEvaluatedTwice()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        var rate = AddRate(
            context,
            data,
            data.SourceA,
            4.40m,
            4.50m,
            new DateTime(2026, 6, 11));
        AddAlert(context, data, AlertType.Threshold, AlertPriceSide.UserSellsCurrency,
            threshold: 4.50m, direction: ThresholdDirection.AboveOrEqual);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var first = await service.EvaluateAllActiveAlertsAsync();
        rate.BuyPrice = 4.60m;
        await context.SaveChangesAsync();
        var second = await service.EvaluateAllActiveAlertsAsync();

        first.TriggeredAlertsCount.Should().Be(0);
        second.TriggeredAlertsCount.Should().Be(0);
        context.Notifications.Should().BeEmpty();
        context.UserAlertEvaluationStates.Should().ContainSingle()
            .Which.LastEvaluatedEffectiveDate.Should().Be(new DateTime(2026, 6, 11));
    }

    [Fact]
    public async Task FulfilledAlert_ContinuesMonitoringAndLogsNewEffectiveDate()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.60m, 4.70m, new DateTime(2026, 6, 11));
        var alert = AddAlert(
            context,
            data,
            AlertType.Threshold,
            AlertPriceSide.UserSellsCurrency,
            threshold: 4.50m,
            direction: ThresholdDirection.AboveOrEqual);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await service.EvaluateAllActiveAlertsAsync();
        AddRate(context, data, data.SourceA, 4.65m, 4.75m, new DateTime(2026, 6, 12));
        await context.SaveChangesAsync();
        var second = await service.EvaluateAllActiveAlertsAsync();

        alert.Status.Should().Be(AlertStatus.Fulfilled);
        second.TriggeredAlertsCount.Should().Be(1);
        context.AlertLogs.Count().Should().Be(2);
        context.Notifications.Count().Should().Be(2);
    }

    [Fact]
    public async Task ThresholdAlert_StatusFollowsLatestRateWhileKeepingHistory()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 3.80m, 3.90m, new DateTime(2026, 6, 11));
        var alert = AddAlert(
            context,
            data,
            AlertType.Threshold,
            AlertPriceSide.UserSellsCurrency,
            threshold: 3.70m,
            direction: ThresholdDirection.AboveOrEqual);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await service.EvaluateAllActiveAlertsAsync();
        alert.Status.Should().Be(AlertStatus.Fulfilled);

        AddRate(context, data, data.SourceA, 3.60m, 3.70m, new DateTime(2026, 6, 12));
        await context.SaveChangesAsync();
        await service.EvaluateAllActiveAlertsAsync();
        alert.Status.Should().Be(AlertStatus.Active);
        context.AlertLogs.Should().ContainSingle();

        AddRate(context, data, data.SourceA, 3.80m, 3.90m, new DateTime(2026, 6, 13));
        await context.SaveChangesAsync();
        await service.EvaluateAllActiveAlertsAsync();

        alert.Status.Should().Be(AlertStatus.Fulfilled);
        context.AlertLogs.Count().Should().Be(2);
        context.Notifications.Count().Should().Be(2);
    }

    [Fact]
    public async Task InactiveAlert_IsNotEvaluated()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.60m, 4.70m, new DateTime(2026, 6, 11));
        var alert = AddAlert(
            context,
            data,
            AlertType.Threshold,
            AlertPriceSide.UserSellsCurrency,
            threshold: 4.50m,
            direction: ThresholdDirection.AboveOrEqual);
        alert.Status = AlertStatus.Inactive;
        await context.SaveChangesAsync();

        var result = await CreateService(context).EvaluateAllActiveAlertsAsync();

        result.EvaluatedAlertsCount.Should().Be(0);
        context.AlertLogs.Should().BeEmpty();
        context.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task PercentageAlertWithoutPreviousRate_DoesNotFailOrTrigger()
    {
        await using var context = CreateContext();
        var data = SeedBase(context);
        AddRate(context, data, data.SourceA, 4.20m, 4.30m, new DateTime(2026, 6, 11));
        AddAlert(context, data, AlertType.PriceIncrease, AlertPriceSide.UserBuysCurrency,
            percentage: 2m);
        await context.SaveChangesAsync();

        var action = async () => await CreateService(context).EvaluateAllActiveAlertsAsync();

        var result = await action.Should().NotThrowAsync();
        result.Subject.TriggeredAlertsCount.Should().Be(0);
        context.Notifications.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static AlertEvaluationService CreateService(ApplicationDbContext context)
    {
        return new AlertEvaluationService(context);
    }

    private static TestData SeedBase(ApplicationDbContext context)
    {
        var user = new User { Id = 1, Username = "user@example.com" };
        var currency = new Currency { Id = 1, Symbol = "EUR", Name = "Euro", IsActive = true };
        var sourceA = new RateSource
        {
            Id = 1,
            Code = "MOCK_BANK_A",
            Name = "Development Mock Bank A",
            IsActive = true
        };
        var sourceB = new RateSource
        {
            Id = 2,
            Code = "MOCK_BANK_B",
            Name = "Development Mock Bank B",
            IsActive = true
        };
        context.AddRange(user, currency, sourceA, sourceB);
        return new TestData(user, currency, sourceA, sourceB);
    }

    private static ExchangeRate AddRate(
        ApplicationDbContext context,
        TestData data,
        RateSource source,
        decimal buyPrice,
        decimal sellPrice,
        DateTime effectiveDate,
        decimal? midPrice = null)
    {
        var rate = new ExchangeRate
        {
            Currency = data.Currency,
            RateSource = source,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            MidPrice = midPrice,
            EffectiveDate = effectiveDate,
            FetchedAt = effectiveDate.AddHours(12)
        };
        context.ExchangeRates.Add(rate);
        return rate;
    }

    private static UserAlert AddAlert(
        ApplicationDbContext context,
        TestData data,
        AlertType type,
        AlertPriceSide priceSide,
        decimal? threshold = null,
        ThresholdDirection? direction = null,
        decimal? percentage = null,
        int? sourceId = null)
    {
        var alert = new UserAlert
        {
            User = data.User,
            Currency = data.Currency,
            AlertType = type,
            PriceSide = priceSide,
            ThresholdValue = threshold,
            ThresholdDirection = direction,
            PercentageChange = percentage,
            TimeFrameHours = type == AlertType.Threshold ? null : 24,
            RateSourceId = sourceId,
            Status = AlertStatus.Active,
            CreatedDate = DateTime.UtcNow
        };
        context.UserAlerts.Add(alert);
        return alert;
    }

    private sealed record TestData(
        User User,
        Currency Currency,
        RateSource SourceA,
        RateSource SourceB);
}
