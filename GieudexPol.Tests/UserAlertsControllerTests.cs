using System.Security.Claims;
using FluentAssertions;
using GieudexPol.API.Controllers;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GieudexPol.Tests;

public class UserAlertsControllerTests
{
    private static readonly Guid AuthId = new("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GetUserAlerts_WithGuidSubject_UsesMappedApplicationUserId()
    {
        var alertService = new Mock<IUserAlertService>();
        var userRepository = CreateUserRepository(userId: 3);
        var alerts = new[]
        {
            new UserAlert
            {
                Id = 9,
                UserId = 3,
                CurrencyId = 1,
                Currency = new Currency { Id = 1, Symbol = "USD" },
                AlertType = AlertType.Threshold,
                ThresholdValue = 4.2m,
                IsActive = true
            }
        };
        alertService
            .Setup(service => service.GetUserAlertsByUserIdAsync(3))
            .ReturnsAsync(alerts);
        var controller = CreateController(alertService.Object, userRepository.Object);

        var result = await controller.GetUserAlertsByUserId(3);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should()
            .BeAssignableTo<IEnumerable<UserAlertDto>>().Subject.Single();
        response.UserId.Should().Be(3);
        response.CurrencySymbol.Should().Be("USD");
        response.AlertType.Should().Be(AlertType.Threshold);
    }

    [Fact]
    public async Task CreateUserAlert_AssignsAuthenticatedUserId()
    {
        var alertService = new Mock<IUserAlertService>();
        var userRepository = CreateUserRepository(userId: 3);
        alertService
            .Setup(service => service.CreateUserAlertAsync(It.IsAny<UserAlert>()))
            .Callback<UserAlert>(alert => alert.Id = 12)
            .Returns(Task.CompletedTask);
        var controller = CreateController(alertService.Object, userRepository.Object);
        var request = new UserAlertCreateDto
        {
            CurrencyId = 2,
            AlertType = AlertType.PriceIncrease,
            PercentageChange = 5m,
            TimeFrameHours = 24
        };

        var result = await controller.CreateUserAlert(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<UserAlertDto>().Subject;
        response.Id.Should().Be(12);
        response.UserId.Should().Be(3);
        alertService.Verify(service => service.CreateUserAlertAsync(
            It.Is<UserAlert>(alert =>
                alert.UserId == 3 &&
                alert.CurrencyId == 2 &&
                alert.AlertType == AlertType.PriceIncrease)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUserAlert_WhenOwnedByDifferentUser_ReturnsForbidden()
    {
        var alertService = new Mock<IUserAlertService>();
        var userRepository = CreateUserRepository(userId: 3);
        alertService
            .Setup(service => service.GetByIdAsync(7))
            .ReturnsAsync(new UserAlert { Id = 7, UserId = 4 });
        var controller = CreateController(alertService.Object, userRepository.Object);

        var result = await controller.DeleteUserAlert(7);

        result.Should().BeOfType<ForbidResult>();
        alertService.Verify(service => service.DeleteUserAlertAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetUserAlerts_WithInvalidSubject_ReturnsUnauthorized()
    {
        var alertService = new Mock<IUserAlertService>();
        var userRepository = new Mock<IUserRepository>();
        var controller = CreateController(alertService.Object, userRepository.Object, "not-a-guid");

        var result = await controller.GetUserAlertsByUserId(3);

        result.Result.Should().BeOfType<UnauthorizedResult>();
        alertService.Verify(
            service => service.GetUserAlertsByUserIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    private static Mock<IUserRepository> CreateUserRepository(int userId)
    {
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(userRepository => userRepository.GetByAuthIdAsync(AuthId))
            .ReturnsAsync(new User
            {
                Id = userId,
                AuthId = AuthId,
                Username = "user@example.com"
            });
        return repository;
    }

    private static UserAlertsController CreateController(
        IUserAlertService alertService,
        IUserRepository userRepository,
        string? subject = null)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, subject ?? AuthId.ToString())],
            "TestAuthentication");

        return new UserAlertsController(alertService, userRepository)
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
