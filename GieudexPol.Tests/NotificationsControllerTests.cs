using System.Security.Claims;
using System.Net;
using FluentAssertions;
using GieudexPol.API.Controllers;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GieudexPol.Tests;

public class NotificationsControllerTests
{
    private static readonly Guid AuthId = new("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task GetMyNotifications_ReturnsOnlyAuthenticatedUsersNotifications()
    {
        var service = new Mock<INotificationService>();
        service.Setup(item => item.GetUserNotificationsAsync(3))
            .ReturnsAsync(new[]
            {
                new Notification { Id = 1, UserId = 3, Message = "mine" }
            });
        var controller = CreateController(service.Object, userId: 3);

        var response = await controller.GetMyNotifications();

        response.Result.Should().BeOfType<OkObjectResult>();
        service.Verify(item => item.GetUserNotificationsAsync(3), Times.Once);
        service.Verify(item => item.GetUserNotificationsAsync(
            It.Is<int>(id => id != 3)), Times.Never);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationDoesNotBelongToUser_ReturnsNotFound()
    {
        var service = new Mock<INotificationService>();
        service.Setup(item => item.MarkNotificationAsReadAsync(9, 3))
            .ReturnsAsync(false);
        var controller = CreateController(service.Object, userId: 3);

        var response = await controller.MarkNotificationAsRead(9);

        response.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationBelongsToUser_ReturnsNoContent()
    {
        var service = new Mock<INotificationService>();
        service.Setup(item => item.MarkNotificationAsReadAsync(9, 3))
            .ReturnsAsync(true);
        var controller = CreateController(service.Object, userId: 3);

        var response = await controller.MarkNotificationAsRead(9);

        response.Should().BeOfType<NoContentResult>();
    }

    private static NotificationsController CreateController(
        INotificationService service,
        int userId)
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(item => item.GetByAuthIdAsync(AuthId))
            .ReturnsAsync(new User { Id = userId, AuthId = AuthId });
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, AuthId.ToString())],
            "TestAuthentication");

        return new NotificationsController(service, userRepository.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }
}

public class NotificationsAuthorizationIntegrationTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public NotificationsAuthorizationIntegrationTests(
        CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UserIdRouteForAnotherUser_DoesNotExist()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/notifications/user/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
