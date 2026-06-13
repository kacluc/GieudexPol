using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GieudexPol.Tests;

public class InstantExchangeServiceTests
{
    [Fact]
    public async Task Preview_PlnToUsd_SelectsBestLiquidSource_WithoutChangingState()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        data.BoeUsd.Balance = 200m;
        await context.SaveChangesAsync();
        var balancesBefore = await context.Wallets
            .AsNoTracking()
            .OrderBy(wallet => wallet.Id)
            .Select(wallet => new
            {
                wallet.Id,
                wallet.Balance,
                wallet.ReservedBalance
            })
            .ToListAsync();
        var service = CreateService(context, 19.50m);

        var result = await service.PreviewAsync(
            data.User.Id,
            data.Pln.Id,
            3900m,
            data.Usd.Id);

        result.RateSourceCode.Should().Be("ECB");
        result.EstimatedOutputAmount.Should().Be(1000m);
        result.Rate.Should().Be(1m / 3.90m);
        result.FeeAmount.Should().Be(19.50m);
        result.TotalDebitAmount.Should().Be(3919.50m);
        result.IsPreview.Should().BeTrue();
        result.HasSufficientFunds.Should().BeTrue();
        (await context.Wallets
            .AsNoTracking()
            .OrderBy(wallet => wallet.Id)
            .Select(wallet => new
            {
                wallet.Id,
                wallet.Balance,
                wallet.ReservedBalance
            })
            .ToListAsync()).Should().BeEquivalentTo(balancesBefore);
        context.Transactions.Should().BeEmpty();
        context.ExchangeExecutions.Should().BeEmpty();
    }

    [Fact]
    public async Task Preview_SelectsBestRateWhenItHasLiquidity()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context, 19.50m);

        var result = await service.PreviewAsync(
            data.User.Id,
            data.Pln.Id,
            3900m,
            data.Usd.Id);

        result.RateSourceCode.Should().Be("BOE");
        result.EstimatedOutputAmount.Should().Be(
            decimal.Round(3900m / 3.88m, 4, MidpointRounding.AwayFromZero));
    }

    [Fact]
    public async Task Preview_RatesOlderThanSevenDays_AreRejected()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        foreach (var rate in context.ExchangeRates)
        {
            rate.EffectiveDate = DateTime.Today.AddDays(-8);
        }
        await context.SaveChangesAsync();
        var service = CreateService(context, 10m);

        var action = () => service.PreviewAsync(
            data.User.Id,
            data.Pln.Id,
            3900m,
            data.Usd.Id);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ostatnich 7 dni*");
    }

    [Fact]
    public async Task Preview_SameCurrency_IsRejected()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context, 10m);

        var action = () => service.PreviewAsync(
            data.User.Id,
            data.Pln.Id,
            100m,
            data.Pln.Id);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*musza byc rozne*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Preview_NonPositiveAmount_IsRejected(decimal amount)
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context, 10m);

        var action = () => service.PreviewAsync(
            data.User.Id,
            data.Pln.Id,
            amount,
            data.Usd.Id);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*wieksza od zera*");
    }

    [Fact]
    public async Task PlnToUsd_SkipsBestRateWithoutLiquidity_AndBooksFee()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        data.BoeUsd.Balance = 200m;
        await context.SaveChangesAsync();
        var service = CreateService(context, 19.50m);

        var result = await service.ExecuteAsync(
            data.User.Id,
            data.Pln.Id,
            3900m,
            data.Usd.Id);

        result.RateSource.Should().Be("ECB");
        result.AmountTo.Should().Be(1000m);
        result.FeeAmount.Should().Be(19.50m);
        data.UserPln.Balance.Should().Be(6080.50m);
        data.UserUsd.Balance.Should().Be(1000m);
        data.EcbUsd.Balance.Should().Be(399_000m);
        data.EcbPln.Balance.Should().Be(1_003_900m);
        data.TreasuryPln.Balance.Should().Be(19.50m);

        var execution = await context.ExchangeExecutions.SingleAsync();
        execution.RateSourceId.Should().Be(data.Ecb.Id);
        execution.FeeAmount.Should().Be(19.50m);
        var transactions = await context.Transactions.ToListAsync();
        transactions.Should().HaveCount(2);
        transactions.Should().OnlyContain(item =>
            item.ExchangeExecutionId == execution.Id);
    }

    [Fact]
    public async Task UsdToPln_SelectsHighestBuyPriceWithLiquidity()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        data.UserUsd.Balance = 2_000m;
        var service = CreateService(context, 10m);

        var result = await service.ExecuteAsync(
            data.User.Id,
            data.Usd.Id,
            1000m,
            data.Pln.Id);

        result.RateSource.Should().Be("BOE");
        result.AmountTo.Should().Be(3880m);
        data.UserUsd.Balance.Should().Be(990m);
        data.UserPln.Balance.Should().Be(13_880m);
        data.BoeUsd.Balance.Should().Be(401_000m);
        data.BoePln.Balance.Should().Be(996_120m);
        data.TreasuryUsd.Balance.Should().Be(10m);
    }

    [Fact]
    public async Task RatesOlderThanSevenDays_AreRejected()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        foreach (var rate in context.ExchangeRates)
        {
            rate.EffectiveDate = DateTime.Today.AddDays(-8);
        }
        await context.SaveChangesAsync();
        var service = CreateService(context, 10m);

        var action = () => service.ExecuteAsync(
            data.User.Id,
            data.Pln.Id,
            3900m,
            data.Usd.Id);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ostatnich 7 dni*");
    }

    private static InstantExchangeService CreateService(
        ApplicationDbContext context,
        decimal fee)
    {
        var calculator = new Mock<ITransactionFeeCalculator>();
        calculator.Setup(item => item.CalculateAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationFeeCalculationDto(fee, null));
        return new InstantExchangeService(
            context,
            calculator.Object,
            new SystemAccountService(context));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static SeedData Seed(ApplicationDbContext context)
    {
        var pln = new Currency { Id = 1, Symbol = "PLN", Name = "PLN", IsActive = true };
        var usd = new Currency { Id = 2, Symbol = "USD", Name = "USD", IsActive = true };
        var user = CreateUser(1, "user@test.local", AccountType.RegularUser);
        var treasury = CreateUser(2, "system_platform_treasury", AccountType.PlatformTreasury);
        var ecbUser = CreateUser(3, "system_ecb", AccountType.RateSourceSystem);
        var boeUser = CreateUser(4, "system_boe", AccountType.RateSourceSystem);
        var ecb = new RateSource
        {
            Id = 1,
            Code = "ECB",
            Name = "ECB",
            IsActive = true,
            SystemUser = ecbUser
        };
        var boe = new RateSource
        {
            Id = 2,
            Code = "BOE",
            Name = "BOE",
            IsActive = true,
            SystemUser = boeUser
        };
        var userPln = new Wallet { User = user, Currency = pln, Balance = 10_000m };
        var userUsd = new Wallet { User = user, Currency = usd, Balance = 0m };
        var treasuryPln = new Wallet { User = treasury, Currency = pln, Balance = 0m };
        var treasuryUsd = new Wallet { User = treasury, Currency = usd, Balance = 0m };
        var ecbPln = new Wallet { User = ecbUser, Currency = pln, Balance = 1_000_000m };
        var ecbUsd = new Wallet { User = ecbUser, Currency = usd, Balance = 400_000m };
        var boePln = new Wallet { User = boeUser, Currency = pln, Balance = 1_000_000m };
        var boeUsd = new Wallet { User = boeUser, Currency = usd, Balance = 400_000m };

        context.AddRange(pln, usd, user, treasury, ecbUser, boeUser, ecb, boe);
        context.Wallets.AddRange(
            userPln,
            userUsd,
            treasuryPln,
            treasuryUsd,
            ecbPln,
            ecbUsd,
            boePln,
            boeUsd);
        context.ExchangeRates.AddRange(
            new ExchangeRate
            {
                Currency = usd,
                RateSource = ecb,
                BuyPrice = 3.85m,
                SellPrice = 3.90m,
                MidPrice = 3.875m,
                EffectiveDate = DateTime.Today,
                FetchedAt = DateTime.UtcNow
            },
            new ExchangeRate
            {
                Currency = usd,
                RateSource = boe,
                BuyPrice = 3.88m,
                SellPrice = 3.88m,
                MidPrice = 3.88m,
                EffectiveDate = DateTime.Today,
                FetchedAt = DateTime.UtcNow
            });
        context.SaveChanges();

        return new SeedData(
            pln,
            usd,
            user,
            treasuryPln,
            treasuryUsd,
            userPln,
            userUsd,
            ecb,
            ecbPln,
            ecbUsd,
            boePln,
            boeUsd);
    }

    private static User CreateUser(
        int id,
        string username,
        AccountType accountType)
    {
        return new User
        {
            Id = id,
            AuthId = Guid.NewGuid(),
            Username = username,
            DisplayName = username,
            PasswordHash = "test",
            Role = accountType == AccountType.RegularUser ? "User" : "System",
            AccountType = accountType
        };
    }

    private sealed record SeedData(
        Currency Pln,
        Currency Usd,
        User User,
        Wallet TreasuryPln,
        Wallet TreasuryUsd,
        Wallet UserPln,
        Wallet UserUsd,
        RateSource Ecb,
        Wallet EcbPln,
        Wallet EcbUsd,
        Wallet BoePln,
        Wallet BoeUsd);
}
