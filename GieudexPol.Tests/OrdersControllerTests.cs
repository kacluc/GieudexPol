using System.Security.Claims;
using FluentAssertions;
using GieudexPol.API.Controllers;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GieudexPol.Tests;

public class OrdersControllerTests
{
    private static readonly Guid AuthId = Guid.NewGuid();

    [Fact]
    public async Task Create_UsesAuthenticatedUserId()
    {
        var service = new Mock<IOrderBookService>();
        var users = CreateUserRepository(userId: 17);
        var request = new CreateOrderRequestDto
        {
            BaseCurrencyCode = "EUR",
            QuoteCurrencyCode = "PLN",
            Side = OrderSide.Buy,
            Price = 4.20m,
            Amount = 10m
        };
        service
            .Setup(item => item.PlaceOrderAsync(
                17,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderDto { Id = 5, Status = OrderStatus.Open });
        var controller = CreateController(service.Object, users.Object);

        var result = await controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<CreatedResult>();
        service.Verify(item => item.PlaceOrderAsync(
            17,
            request,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_UsesAuthenticatedUserId()
    {
        var service = new Mock<IOrderBookService>();
        var users = CreateUserRepository(userId: 23);
        var controller = CreateController(service.Object, users.Object);

        var result = await controller.Cancel(91, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        service.Verify(item => item.CancelOrderAsync(
            23,
            91,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Controller_RequiresAuthorization()
    {
        typeof(OrdersController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Should()
            .ContainSingle();
    }

    private static Mock<IUserRepository> CreateUserRepository(int userId)
    {
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(item => item.GetByAuthIdAsync(AuthId))
            .ReturnsAsync(new User
            {
                Id = userId,
                AuthId = AuthId,
                Username = $"user{userId}@test.local"
            });
        return repository;
    }

    private static OrdersController CreateController(
        IOrderBookService service,
        IUserRepository users)
    {
        var controller = new OrdersController(service, users);
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, AuthId.ToString())],
            "TestAuthentication");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
        return controller;
    }
}
