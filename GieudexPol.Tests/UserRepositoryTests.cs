using FluentAssertions;
using GieudexPol.Domain;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using AuthUser = GieudexPol.Domain.Auth.User;

namespace GieudexPol.Tests;

public class UserRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldCreateZeroBalancePlnWallet()
    {
        await using var context = CreateContext();
        var pln = new Currency
        {
            Symbol = TradingCurrencyCatalog.BaseCurrencySymbol,
            Name = "Polski zloty",
            IsActive = true
        };
        await context.Currencies.AddAsync(pln);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context);
        var user = new AuthUser(
            Guid.NewGuid(),
            "new-user@example.com",
            "Password123!",
            displayName: "Nowy User");

        await repository.AddAsync(user);

        var persistedUser = await context.Users
            .Include(item => item.Wallets)
            .ThenInclude(wallet => wallet.Currency)
            .SingleAsync(item => item.Username == user.Email);
        persistedUser.DisplayName.Should().Be("Nowy User");
        persistedUser.Wallets.Should().ContainSingle();
        persistedUser.Wallets.Single().Currency.Symbol.Should()
            .Be(TradingCurrencyCatalog.BaseCurrencySymbol);
        persistedUser.Wallets.Single().Balance.Should().Be(0m);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
