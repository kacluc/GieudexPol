using System.Net;
using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Exceptions;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class AdminTestExchangeRateServiceTests
{
    [Fact]
    public async Task GetRatesAsync_ReturnsOnlyDevelopmentRatesAndAppliesFilters()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var result = await service.GetRatesAsync(
            data.Eur.Id,
            null,
            new DateTime(2026, 6, 10),
            new DateTime(2026, 6, 10));

        result.Should().ContainSingle();
        result[0].CurrencyCode.Should().Be("EUR");
        result[0].RateSourceCode.Should().Be(DevelopmentIdentity.RateSourceCode);
    }

    [Fact]
    public async Task CreateRateAsync_CreatesDevelopmentRateAndCalculatesMidPrice()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var result = await service.CreateRateAsync(new CreateTestExchangeRateDto
        {
            CurrencyCode = "USD",
            EffectiveDate = new DateTime(2026, 6, 11),
            BuyPrice = 3.90m,
            SellPrice = 4.10m
        });

        result.MidPrice.Should().Be(4.00m);
        result.RateSourceCode.Should().Be(DevelopmentIdentity.RateSourceCode);
        var stored = await context.ExchangeRates.SingleAsync(
            rate => rate.Id == result.Id);
        stored.RateSourceId.Should().Be(data.DevelopmentSource.Id);
    }

    [Fact]
    public async Task UpdateRateAsync_UpdatesDevelopmentRate()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var result = await service.UpdateRateAsync(
            data.DevelopmentRate.Id,
            new UpdateTestExchangeRateDto
            {
                EffectiveDate = new DateTime(2026, 6, 12),
                BuyPrice = 4.20m,
                SellPrice = 4.40m,
                MidPrice = 4.31m
            });

        result.Should().NotBeNull();
        result!.EffectiveDate.Should().Be(new DateTime(2026, 6, 12));
        result.MidPrice.Should().Be(4.31m);
    }

    [Fact]
    public async Task DeleteRateAsync_DeletesDevelopmentRate()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var deleted = await service.DeleteRateAsync(data.DevelopmentRate.Id);

        deleted.Should().BeTrue();
        var stillExists = await context.ExchangeRates.AnyAsync(
            rate => rate.Id == data.DevelopmentRate.Id);
        stillExists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateRateAsync_RejectsRateFromRealSource()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var action = () => service.UpdateRateAsync(
            data.RealRate.Id,
            new UpdateTestExchangeRateDto
            {
                EffectiveDate = new DateTime(2026, 6, 11),
                BuyPrice = 4.00m,
                SellPrice = 4.20m
            });

        await action.Should().ThrowAsync<ProtectedExchangeRateException>();
    }

    [Fact]
    public async Task DeleteRateAsync_RejectsRateFromRealSource()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var action = () => service.DeleteRateAsync(data.RealRate.Id);

        await action.Should().ThrowAsync<ProtectedExchangeRateException>();
    }

    [Fact]
    public async Task CreateRateAsync_RejectsNegativePrice()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var action = () => service.CreateRateAsync(new CreateTestExchangeRateDto
        {
            CurrencyCode = "USD",
            EffectiveDate = new DateTime(2026, 6, 11),
            BuyPrice = -1m,
            SellPrice = 4.10m
        });

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*dodatnie*");
    }

    [Fact]
    public async Task CreateRateAsync_RejectsMissingCurrency()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var action = () => service.CreateRateAsync(new CreateTestExchangeRateDto
        {
            CurrencyCode = "XYZ",
            EffectiveDate = new DateTime(2026, 6, 11),
            BuyPrice = 1m,
            SellPrice = 1.10m
        });

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*waluta nie istnieje*");
    }

    [Fact]
    public async Task CreateRateAsync_RejectsDuplicateCurrencySourceAndDate()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = new AdminTestExchangeRateService(context);

        var action = () => service.CreateRateAsync(new CreateTestExchangeRateDto
        {
            CurrencyCode = "EUR",
            EffectiveDate = new DateTime(2026, 6, 10),
            BuyPrice = 4.00m,
            SellPrice = 4.20m
        });

        await action.Should().ThrowAsync<TestExchangeRateConflictException>();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<SeededData> SeedAsync(ApplicationDbContext context)
    {
        var eur = new Currency { Symbol = "EUR", Name = "Euro", IsActive = true };
        var usd = new Currency { Symbol = "USD", Name = "US Dollar", IsActive = true };
        var developmentSource = new RateSource
        {
            Code = DevelopmentIdentity.RateSourceCode,
            Name = "Development Mock Bank A",
            IsActive = true
        };
        var realSource = new RateSource
        {
            Code = "NBP",
            Name = "Narodowy Bank Polski",
            IsActive = true
        };

        context.AddRange(eur, usd, developmentSource, realSource);
        await context.SaveChangesAsync();

        var developmentRate = new ExchangeRate
        {
            CurrencyId = eur.Id,
            RateSourceId = developmentSource.Id,
            BuyPrice = 4.10m,
            SellPrice = 4.30m,
            MidPrice = 4.20m,
            EffectiveDate = new DateTime(2026, 6, 10),
            FetchedAt = DateTime.UtcNow
        };
        var realRate = new ExchangeRate
        {
            CurrencyId = eur.Id,
            RateSourceId = realSource.Id,
            BuyPrice = 4.11m,
            SellPrice = 4.31m,
            MidPrice = 4.21m,
            EffectiveDate = new DateTime(2026, 6, 10),
            FetchedAt = DateTime.UtcNow
        };

        context.AddRange(developmentRate, realRate);
        await context.SaveChangesAsync();

        return new SeededData(
            eur,
            usd,
            developmentSource,
            developmentRate,
            realRate);
    }

    private sealed record SeededData(
        Currency Eur,
        Currency Usd,
        RateSource DevelopmentSource,
        ExchangeRate DevelopmentRate,
        ExchangeRate RealRate);
}

public class AdminTestExchangeRatesAuthorizationTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AdminTestExchangeRatesAuthorizationTests(
        CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRates_AuthenticatedUserWithoutAdminRole_ReturnsForbidden()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/admin/test-exchange-rates");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
