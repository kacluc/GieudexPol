using FluentAssertions;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class CurrencyRepositoryTests
{
    [Fact]
    public async Task GetTradableCurrenciesAsync_ShouldIncludePlnWithoutExchangeRates()
    {
        await using var context = CreateContext();
        await context.Currencies.AddRangeAsync(
            new Currency { Symbol = "PLN", Name = "Polski zloty", IsActive = true },
            new Currency { Symbol = "EUR", Name = "Euro", IsActive = true });
        await context.SaveChangesAsync();
        var repository = new CurrencyRepository(context);

        var result = await repository.GetTradableCurrenciesAsync();

        result.Select(currency => currency.Symbol).Should().Equal("PLN");
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
