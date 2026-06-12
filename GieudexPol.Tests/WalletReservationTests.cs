using FluentAssertions;
using GieudexPol.Domain.Entities;

namespace GieudexPol.Tests;

public class WalletReservationTests
{
    [Fact]
    public void Debit_CannotSpendReservedBalance()
    {
        var wallet = new Wallet
        {
            Balance = 100m,
            ReservedBalance = 80m
        };

        var action = () => wallet.Debit(20.01m);

        action.Should().Throw<InvalidOperationException>();
        wallet.Balance.Should().Be(100m);
        wallet.ReservedBalance.Should().Be(80m);
    }

    [Fact]
    public void Debit_AllowsSpendingAvailableBalanceOnly()
    {
        var wallet = new Wallet
        {
            Balance = 100m,
            ReservedBalance = 80m
        };

        wallet.Debit(20m);

        wallet.Balance.Should().Be(80m);
        wallet.ReservedBalance.Should().Be(80m);
        wallet.AvailableBalance.Should().Be(0m);
    }
}
