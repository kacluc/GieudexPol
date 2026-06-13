using FluentAssertions;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Repositories;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class PlatformTreasuryBookingTests
{
    [Theory]
    [InlineData(90, 140)]
    [InlineData(-110, 40)]
    public async Task BalanceOperation_CreditsFeeToTreasury(
        decimal balanceChange,
        decimal expectedUserBalance)
    {
        await using var context = CreateContext();
        var data = Seed(context);
        if (balanceChange < 0)
        {
            data.UserWallet.Balance = 150m;
            await context.SaveChangesAsync();
        }
        var repository = new WalletRepository(
            context,
            new SystemAccountService(context));

        await repository.ExecuteBalanceOperationAsync(
            data.UserWallet.Id,
            balanceChange,
            Transaction(
                data.User.Id,
                data.Pln.Id,
                balanceChange > 0 ? "Deposit" : "Withdrawal",
                10m));

        (await context.Wallets.SingleAsync(wallet =>
            wallet.Id == data.UserWallet.Id)).Balance.Should().Be(expectedUserBalance);
        (await context.Wallets.SingleAsync(wallet =>
            wallet.Id == data.TreasuryWallet.Id)).Balance.Should().Be(10m);
    }

    [Fact]
    public async Task Transfer_CreditsReceiverAndTreasury_AndDebitsAvailableBalance()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        data.UserWallet.Balance = 200m;
        data.UserWallet.ReservedBalance = 50m;
        await context.SaveChangesAsync();
        var repository = new WalletRepository(
            context,
            new SystemAccountService(context));

        await repository.ExecuteTransferAsync(
            data.UserWallet.Id,
            data.Receiver.Id,
            data.Pln.Id,
            100m,
            10m,
            Transaction(data.User.Id, data.Pln.Id, "Transfer", 10m, data.Receiver.Id));

        var userWallet = await context.Wallets.SingleAsync(wallet =>
            wallet.Id == data.UserWallet.Id);
        userWallet.Balance.Should().Be(90m);
        userWallet.ReservedBalance.Should().Be(50m);
        (await context.Wallets.SingleAsync(wallet =>
            wallet.Id == data.ReceiverWallet.Id)).Balance.Should().Be(100m);
        (await context.Wallets.SingleAsync(wallet =>
            wallet.Id == data.TreasuryWallet.Id)).Balance.Should().Be(10m);
    }

    private static Transaction Transaction(
        int senderId,
        int currencyId,
        string type,
        decimal fee,
        int? receiverId = null)
    {
        return new Transaction
        {
            SenderId = senderId,
            ReceiverId = receiverId ?? senderId,
            CurrencyId = currencyId,
            Amount = 100m,
            AppliedFee = fee,
            TransactionType = type,
            Status = "Completed",
            Timestamp = DateTime.UtcNow
        };
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
        var pln = new Currency
        {
            Id = 1,
            Symbol = "PLN",
            Name = "PLN",
            IsActive = true
        };
        var user = User(1, "user@test.local", AccountType.RegularUser);
        var receiver = User(2, "receiver@test.local", AccountType.RegularUser);
        var treasury = User(3, "system_platform_treasury", AccountType.PlatformTreasury);
        var userWallet = new Wallet { User = user, Currency = pln, Balance = 50m };
        var receiverWallet = new Wallet { User = receiver, Currency = pln, Balance = 0m };
        var treasuryWallet = new Wallet { User = treasury, Currency = pln, Balance = 0m };
        context.AddRange(pln, user, receiver, treasury);
        context.Wallets.AddRange(userWallet, receiverWallet, treasuryWallet);
        context.SaveChanges();
        return new SeedData(
            pln,
            user,
            receiver,
            userWallet,
            receiverWallet,
            treasuryWallet);
    }

    private static User User(
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
        User User,
        User Receiver,
        Wallet UserWallet,
        Wallet ReceiverWallet,
        Wallet TreasuryWallet);
}
