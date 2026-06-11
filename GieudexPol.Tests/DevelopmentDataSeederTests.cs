using FluentAssertions;
using GieudexPol.Domain;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GieudexPol.Tests;

public class DevelopmentDataSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesTwoDevelopmentSourcesWithDistinctRatesIdempotently()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        await using var serviceProvider = services.BuildServiceProvider();

        await using (var setupScope = serviceProvider.CreateAsyncScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await setupContext.Database.EnsureCreatedAsync();
        }

        await RunSeederAsync(serviceProvider);
        var countsAfterFirstRun = await GetDevelopmentDataCountsAsync(serviceProvider);
        await RunSeederAsync(serviceProvider);

        await using var verificationScope = serviceProvider.CreateAsyncScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sources = await context.RateSources
            .Where(source =>
                source.Code == DevelopmentIdentity.RateSourceCode ||
                source.Code == DevelopmentIdentity.RateSourceCodeB)
            .ToListAsync();

        sources.Should().HaveCount(2);
        sources.Should().ContainSingle(source =>
            source.Code == DevelopmentIdentity.RateSourceCode &&
            source.Name == "Development Mock Bank A" &&
            source.IsActive);
        sources.Should().ContainSingle(source =>
            source.Code == DevelopmentIdentity.RateSourceCodeB &&
            source.Name == "Development Mock Bank B" &&
            source.IsActive);

        var sourceA = sources.Single(source =>
            source.Code == DevelopmentIdentity.RateSourceCode);
        var sourceB = sources.Single(source =>
            source.Code == DevelopmentIdentity.RateSourceCodeB);
        var ratesA = await context.ExchangeRates
            .Where(rate => rate.RateSourceId == sourceA.Id)
            .ToListAsync();
        var ratesB = await context.ExchangeRates
            .Where(rate => rate.RateSourceId == sourceB.Id)
            .ToListAsync();

        ratesA.Should().NotBeEmpty();
        ratesB.Should().HaveCount(ratesA.Count);
        sources.Count.Should().Be(countsAfterFirstRun.SourceCount);
        ratesA.Count.Should().Be(countsAfterFirstRun.RateCountA);
        ratesB.Count.Should().Be(countsAfterFirstRun.RateCountB);

        var basicCurrencyIds = await context.Currencies
            .Where(currency =>
                currency.Symbol == "EUR" ||
                currency.Symbol == "USD" ||
                currency.Symbol == "GBP")
            .Select(currency => currency.Id)
            .ToListAsync();
        ratesB.Select(rate => rate.CurrencyId)
            .Distinct()
            .Should()
            .Contain(basicCurrencyIds);

        var comparableRateA = ratesA
            .OrderBy(rate => rate.EffectiveDate)
            .First(rate => rate.CurrencyId == basicCurrencyIds[0]);
        var comparableRateB = ratesB.Single(rate =>
            rate.CurrencyId == comparableRateA.CurrencyId &&
            rate.EffectiveDate == comparableRateA.EffectiveDate);
        comparableRateB.MidPrice.Should().NotBe(comparableRateA.MidPrice);
    }

    private static async Task RunSeederAsync(ServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await DevelopmentDataSeeder.SeedAsync(scope.ServiceProvider);
    }

    private static async Task<(int SourceCount, int RateCountA, int RateCountB)>
        GetDevelopmentDataCountsAsync(ServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sources = await context.RateSources
            .Where(source =>
                source.Code == DevelopmentIdentity.RateSourceCode ||
                source.Code == DevelopmentIdentity.RateSourceCodeB)
            .ToListAsync();
        var sourceAId = sources.Single(source =>
            source.Code == DevelopmentIdentity.RateSourceCode).Id;
        var sourceBId = sources.Single(source =>
            source.Code == DevelopmentIdentity.RateSourceCodeB).Id;

        return (
            sources.Count,
            await context.ExchangeRates.CountAsync(rate => rate.RateSourceId == sourceAId),
            await context.ExchangeRates.CountAsync(rate => rate.RateSourceId == sourceBId));
    }
}
