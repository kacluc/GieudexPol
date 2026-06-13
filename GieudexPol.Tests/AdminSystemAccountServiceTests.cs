using FluentAssertions;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class AdminSystemAccountServiceTests
{
    [Fact]
    public async Task GetAccountsAsync_ReturnsSourceLiquidityAndTreasuryBalances()
    {
        await using var context = CreateContext();
        var pln = new Currency
        {
            Symbol = "PLN",
            Name = "Polski złoty",
            IsActive = true
        };
        var eur = new Currency
        {
            Symbol = "EUR",
            Name = "Euro",
            IsActive = true
        };
        var sourceUser = CreateUser(
            "system_ecb",
            AccountType.RateSourceSystem);
        var treasury = CreateUser(
            "system_platform_treasury",
            AccountType.PlatformTreasury);
        var regularUser = CreateUser(
            "user@example.com",
            AccountType.RegularUser);
        var source = new RateSource
        {
            Code = "ECB",
            Name = "European Central Bank",
            IsActive = true,
            SystemUser = sourceUser
        };
        context.AddRange(pln, eur, sourceUser, treasury, regularUser, source);
        context.Wallets.AddRange(
            new Wallet
            {
                User = sourceUser,
                Currency = eur,
                Balance = 400_000m,
                ReservedBalance = 25_000m
            },
            new Wallet
            {
                User = treasury,
                Currency = pln,
                Balance = 125.50m
            },
            new Wallet
            {
                User = regularUser,
                Currency = pln,
                Balance = 1_000m
            });
        await context.SaveChangesAsync();
        var service = new AdminSystemAccountService(context);

        var result = await service.GetAccountsAsync();

        result.Should().HaveCount(2);
        var sourceAccount = result.Single(item =>
            item.AccountType == nameof(AccountType.RateSourceSystem));
        sourceAccount.RateSourceCode.Should().Be("ECB");
        sourceAccount.RateSourceIsActive.Should().BeTrue();
        sourceAccount.Wallets.Should().ContainSingle(wallet =>
            wallet.CurrencyCode == "EUR" &&
            wallet.Balance == 400_000m &&
            wallet.ReservedBalance == 25_000m &&
            wallet.AvailableBalance == 375_000m);

        var treasuryAccount = result.Single(item =>
            item.AccountType == nameof(AccountType.PlatformTreasury));
        treasuryAccount.RateSourceCode.Should().BeNull();
        treasuryAccount.Wallets.Should().ContainSingle(wallet =>
            wallet.CurrencyCode == "PLN" &&
            wallet.AvailableBalance == 125.50m);
        result.Should().NotContain(item => item.Username == regularUser.Username);
    }

    private static User CreateUser(string username, AccountType accountType)
    {
        return new User
        {
            AuthId = Guid.NewGuid(),
            Username = username,
            DisplayName = username,
            PasswordHash = "test",
            Role = accountType == AccountType.RegularUser ? "User" : "System",
            AccountType = accountType
        };
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
