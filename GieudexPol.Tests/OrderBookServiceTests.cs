using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class OrderBookServiceTests
{
    [Fact]
    public async Task Buy_MatchesSellAtSamePrice_AndPartiallyFillsSell()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        var sell = await service.PlaceOrderAsync(
            data.Seller.Id,
            Request(OrderSide.Sell, 4.30m, 100m));
        var buy = await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.30m, 50m));

        buy.Status.Should().Be(OrderStatus.Filled);
        buy.RemainingAmount.Should().Be(0m);
        sell.Status.Should().Be(OrderStatus.Open);

        var persistedSell = await context.Orders.SingleAsync(order => order.Id == sell.Id);
        persistedSell.Status.Should().Be(OrderStatus.PartiallyFilled);
        persistedSell.RemainingAmount.Should().Be(50m);

        var buyerPln = await Wallet(context, data.Buyer.Id, data.Pln.Id);
        var buyerEur = await Wallet(context, data.Buyer.Id, data.Eur.Id);
        var sellerPln = await Wallet(context, data.Seller.Id, data.Pln.Id);
        var sellerEur = await Wallet(context, data.Seller.Id, data.Eur.Id);

        buyerPln.Balance.Should().Be(785m);
        buyerPln.ReservedBalance.Should().Be(0m);
        buyerEur.Balance.Should().Be(50m);
        sellerPln.Balance.Should().Be(215m);
        sellerEur.Balance.Should().Be(50m);
        sellerEur.ReservedBalance.Should().Be(50m);
        (await context.TradeExecutions.CountAsync()).Should().Be(1);
        (await context.Transactions.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Buy_DoesNotMatchSell_WhenSellPriceIsHigher()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        var sell = await service.PlaceOrderAsync(
            data.Seller.Id,
            Request(OrderSide.Sell, 4.30m, 25m));
        var buy = await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.20m, 25m));

        sell.Status.Should().Be(OrderStatus.Open);
        buy.Status.Should().Be(OrderStatus.Open);
        (await context.TradeExecutions.CountAsync()).Should().Be(0);

        var buyerPln = await Wallet(context, data.Buyer.Id, data.Pln.Id);
        buyerPln.ReservedBalance.Should().Be(105m);
    }

    [Fact]
    public async Task Sell_MatchesBuy_WhenBuyPriceIsHigher_AtRestingOrderPrice()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        var buy = await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.30m, 20m));
        var sell = await service.PlaceOrderAsync(
            data.Seller.Id,
            Request(OrderSide.Sell, 4.20m, 20m));

        buy.Status.Should().Be(OrderStatus.Open);
        sell.Status.Should().Be(OrderStatus.Filled);

        var execution = await context.TradeExecutions.SingleAsync();
        execution.Price.Should().Be(4.30m);
        execution.Amount.Should().Be(20m);
        (await context.Orders.SingleAsync(order => order.Id == buy.Id))
            .Status.Should().Be(OrderStatus.Filled);
    }

    [Fact]
    public async Task UnfilledRemainder_RemainsInAggregatedOrderBook()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        await service.PlaceOrderAsync(
            data.Seller.Id,
            Request(OrderSide.Sell, 4.30m, 100m));
        await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.30m, 50m));

        var book = await service.GetOrderBookAsync("EUR", "PLN", 10);

        book.BuyOrders.Should().BeEmpty();
        book.SellOrders.Should().ContainSingle();
        book.SellOrders[0].Price.Should().Be(4.30m);
        book.SellOrders[0].Amount.Should().Be(50m);
        book.SellOrders[0].Total.Should().Be(50m);
        book.SellOrders[0].OrdersCount.Should().Be(1);
    }

    [Fact]
    public async Task OrderBook_GroupsPriceLevels_AndSortsBothSides()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        await service.PlaceOrderAsync(data.Buyer.Id, Request(OrderSide.Buy, 4.10m, 10m));
        await service.PlaceOrderAsync(data.Buyer2.Id, Request(OrderSide.Buy, 4.20m, 15m));
        await service.PlaceOrderAsync(data.Buyer3.Id, Request(OrderSide.Buy, 4.20m, 5m));
        await service.PlaceOrderAsync(data.Seller.Id, Request(OrderSide.Sell, 4.40m, 7m));
        await service.PlaceOrderAsync(data.Seller2.Id, Request(OrderSide.Sell, 4.30m, 9m));

        var book = await service.GetOrderBookAsync("EUR", "PLN", 10);

        book.BuyOrders.Select(level => level.Price).Should().Equal(4.20m, 4.10m);
        book.BuyOrders[0].Amount.Should().Be(20m);
        book.BuyOrders[0].OrdersCount.Should().Be(2);
        book.BuyOrders[1].Total.Should().Be(30m);
        book.SellOrders.Select(level => level.Price).Should().Equal(4.30m, 4.40m);
        book.SellOrders[1].Total.Should().Be(16m);
    }

    [Fact]
    public async Task Buy_MatchesLowestSellFirst_ThenOldestAtTheSamePrice()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        var olderAtBestPrice = await service.PlaceOrderAsync(
            data.Seller.Id,
            Request(OrderSide.Sell, 4.10m, 5m));
        var newerAtBestPrice = await service.PlaceOrderAsync(
            data.Seller2.Id,
            Request(OrderSide.Sell, 4.10m, 5m));
        await service.PlaceOrderAsync(
            data.Seller3.Id,
            Request(OrderSide.Sell, 4.20m, 5m));

        await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.20m, 8m));

        var executions = await context.TradeExecutions
            .OrderBy(execution => execution.Id)
            .ToListAsync();
        executions.Should().HaveCount(2);
        executions[0].SellOrderId.Should().Be(olderAtBestPrice.Id);
        executions[0].Price.Should().Be(4.10m);
        executions[0].Amount.Should().Be(5m);
        executions[1].SellOrderId.Should().Be(newerAtBestPrice.Id);
        executions[1].Price.Should().Be(4.10m);
        executions[1].Amount.Should().Be(3m);
    }

    [Fact]
    public async Task Sell_MatchesHighestBuyFirst()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.10m, 5m));
        var bestBuy = await service.PlaceOrderAsync(
            data.Buyer2.Id,
            Request(OrderSide.Buy, 4.20m, 5m));

        await service.PlaceOrderAsync(
            data.Seller.Id,
            Request(OrderSide.Sell, 4.10m, 3m));

        var execution = await context.TradeExecutions.SingleAsync();
        execution.BuyOrderId.Should().Be(bestBuy.Id);
        execution.Price.Should().Be(4.20m);
        execution.Amount.Should().Be(3m);
    }

    [Fact]
    public async Task PartialBuy_KeepsOnlyRemainingLimitReservation_AndReleasesPriceImprovement()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        await service.PlaceOrderAsync(
            data.Seller.Id,
            Request(OrderSide.Sell, 4.20m, 10m));
        var buy = await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.30m, 20m));

        buy.Status.Should().Be(OrderStatus.PartiallyFilled);
        buy.RemainingAmount.Should().Be(10m);

        var buyerPln = await Wallet(context, data.Buyer.Id, data.Pln.Id);
        buyerPln.Balance.Should().Be(958m);
        buyerPln.ReservedBalance.Should().Be(43m);
        buyerPln.AvailableBalance.Should().Be(915m);
    }

    [Fact]
    public async Task Buy_ReservesQuoteCurrency_AndSellReservesBaseCurrency()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.20m, 100m));
        await service.PlaceOrderAsync(
            data.Seller.Id,
            Request(OrderSide.Sell, 4.40m, 40m));

        (await Wallet(context, data.Buyer.Id, data.Pln.Id))
            .ReservedBalance.Should().Be(420m);
        (await Wallet(context, data.Seller.Id, data.Eur.Id))
            .ReservedBalance.Should().Be(40m);
    }

    [Fact]
    public async Task Cancel_ReleasesReservation_AndOnlyOwnerCanCancel()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        var order = await service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.20m, 100m));

        var foreignCancel = () => service.CancelOrderAsync(data.Seller.Id, order.Id);
        await foreignCancel.Should().ThrowAsync<KeyNotFoundException>();

        await service.CancelOrderAsync(data.Buyer.Id, order.Id);

        var persistedOrder = await context.Orders.SingleAsync(item => item.Id == order.Id);
        persistedOrder.Status.Should().Be(OrderStatus.Cancelled);
        persistedOrder.ClosedAt.Should().NotBeNull();
        (await Wallet(context, data.Buyer.Id, data.Pln.Id))
            .ReservedBalance.Should().Be(0m);
    }

    [Fact]
    public async Task OrderWithoutAvailableFunds_IsRejected()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        var action = () => service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.20m, 300m));

        await action.Should().ThrowAsync<InvalidOperationException>();
        (await context.Orders.CountAsync()).Should().Be(0);
        (await Wallet(context, data.Buyer.Id, data.Pln.Id))
            .ReservedBalance.Should().Be(0m);
    }

    [Fact]
    public void CreateOrderRequest_DoesNotAcceptUserId()
    {
        typeof(CreateOrderRequestDto).GetProperty("UserId").Should().BeNull();
    }

    [Fact]
    public async Task AmountWithMoreThanFourDecimalPlaces_IsRejected()
    {
        await using var context = CreateContext();
        var data = Seed(context);
        var service = CreateService(context);

        var action = () => service.PlaceOrderAsync(
            data.Buyer.Id,
            Request(OrderSide.Buy, 4.20m, 1.00001m));

        await action.Should().ThrowAsync<ArgumentException>();
        (await context.Orders.CountAsync()).Should().Be(0);
    }

    private static OrderBookService CreateService(ApplicationDbContext context)
    {
        return new OrderBookService(context, new OrderMatchingService(context));
    }

    private static CreateOrderRequestDto Request(
        OrderSide side,
        decimal price,
        decimal amount)
    {
        return new CreateOrderRequestDto
        {
            BaseCurrencyCode = "EUR",
            QuoteCurrencyCode = "PLN",
            Side = side,
            Price = price,
            Amount = amount
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
        var eur = new Currency { Id = 1, Symbol = "EUR", Name = "Euro", IsActive = true };
        var pln = new Currency { Id = 2, Symbol = "PLN", Name = "Polski zloty", IsActive = true };
        var buyer = User(1, "buyer@test.local");
        var seller = User(2, "seller@test.local");
        var buyer2 = User(3, "buyer2@test.local");
        var buyer3 = User(4, "buyer3@test.local");
        var seller2 = User(5, "seller2@test.local");
        var seller3 = User(6, "seller3@test.local");
        var pair = new TradingPair
        {
            Id = 1,
            BaseCurrency = eur,
            QuoteCurrency = pln,
            IsActive = true,
            TickSize = 0.0001m
        };

        context.AddRange(
            eur,
            pln,
            buyer,
            seller,
            buyer2,
            buyer3,
            seller2,
            seller3,
            pair);
        context.Wallets.AddRange(
            new Wallet { User = buyer, Currency = pln, Balance = 1000m },
            new Wallet { User = seller, Currency = eur, Balance = 100m },
            new Wallet { User = buyer2, Currency = pln, Balance = 1000m },
            new Wallet { User = buyer3, Currency = pln, Balance = 1000m },
            new Wallet { User = seller2, Currency = eur, Balance = 100m },
            new Wallet { User = seller3, Currency = eur, Balance = 100m });
        context.SaveChanges();

        return new SeedData(
            eur,
            pln,
            buyer,
            seller,
            buyer2,
            buyer3,
            seller2,
            seller3);
    }

    private static User User(int id, string username)
    {
        return new User
        {
            Id = id,
            AuthId = Guid.NewGuid(),
            Username = username,
            DisplayName = username,
            Role = "User"
        };
    }

    private static Task<Wallet> Wallet(
        ApplicationDbContext context,
        int userId,
        int currencyId)
    {
        return context.Wallets.SingleAsync(
            wallet => wallet.UserId == userId && wallet.CurrencyId == currencyId);
    }

    private sealed record SeedData(
        Currency Eur,
        Currency Pln,
        User Buyer,
        User Seller,
        User Buyer2,
        User Buyer3,
        User Seller2,
        User Seller3);
}
