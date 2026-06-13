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
                Status = AlertStatus.Active
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
            .Setup(service => service.CreateUserAlertAsync(
                It.IsAny<UserAlert>(),
                false))
            .Callback<UserAlert, bool>((alert, _) => alert.Id = 12)
            .Returns(Task.CompletedTask);
        var controller = CreateController(alertService.Object, userRepository.Object);
        var request = new UserAlertCreateDto
        {
            CurrencyId = 2,
            AlertType = AlertType.PriceIncrease,
            PriceSide = AlertPriceSide.UserBuysCurrency,
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
                alert.AlertType == AlertType.PriceIncrease &&
                alert.PriceSide == AlertPriceSide.UserBuysCurrency),
            false),
            Times.Once);
    }

    [Fact]
    public async Task GetRateSources_UserDoesNotReceiveMockBanks()
    {
        var alertService = new Mock<IUserAlertService>();
        alertService
            .Setup(service => service.GetActiveRateSourcesAsync(false))
            .ReturnsAsync([
                new RateSource { Id = 1, Code = "NBP", Name = "NBP" }
            ]);
        var controller = CreateController(
            alertService.Object,
            CreateUserRepository(userId: 3).Object);

        var result = await controller.GetRateSources();

        var response = result.Result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeAssignableTo<IEnumerable<AlertRateSourceDto>>().Subject;
        response.Should().ContainSingle(source => source.Code == "NBP");
        alertService.Verify(service => service.GetActiveRateSourcesAsync(false), Times.Once);
    }

    [Fact]
    public async Task GetRateSources_AdminReceivesTestSources()
    {
        var alertService = new Mock<IUserAlertService>();
        alertService
            .Setup(service => service.GetActiveRateSourcesAsync(true))
            .ReturnsAsync([
                new RateSource { Id = 1, Code = "MOCK_BANK_A", Name = "Mock A" }
            ]);
        var controller = CreateController(
            alertService.Object,
            CreateUserRepository(userId: 3).Object,
            role: UserRoles.Admin);

        var result = await controller.GetRateSources();

        var response = result.Result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeAssignableTo<IEnumerable<AlertRateSourceDto>>().Subject;
        response.Should().ContainSingle(source => source.Code == "MOCK_BANK_A");
        alertService.Verify(service => service.GetActiveRateSourcesAsync(true), Times.Once);
    }

    [Fact]
    public async Task GetMyAlerts_UserDoesNotReceiveMockAlertOrMockLogs()
    {
        var alertService = new Mock<IUserAlertService>();
        var mockSource = new RateSource
        {
            Id = 2,
            Code = "MOCK_BANK_A",
            Name = "Mock A"
        };
        alertService
            .Setup(service => service.GetUserAlertsByUserIdAsync(3))
            .ReturnsAsync([
                new UserAlert
                {
                    Id = 1,
                    UserId = 3,
                    CurrencyId = 1,
                    Currency = new Currency { Id = 1, Symbol = "EUR" },
                    RateSourceId = mockSource.Id,
                    RateSource = mockSource,
                    Status = AlertStatus.Active
                },
                new UserAlert
                {
                    Id = 2,
                    UserId = 3,
                    CurrencyId = 1,
                    Currency = new Currency { Id = 1, Symbol = "EUR" },
                    Status = AlertStatus.Fulfilled,
                    Logs =
                    [
                        new AlertLog
                        {
                            Id = 1,
                            Message = "Warunek spelniony wedlug MOCK_BANK_B.",
                            SourceSummary = "MOCK_BANK_B"
                        },
                        new AlertLog
                        {
                            Id = 2,
                            Message = "Warunek spelniony wedlug NBP.",
                            SourceSummary = "NBP"
                        }
                    ]
                }
            ]);
        var controller = CreateController(
            alertService.Object,
            CreateUserRepository(userId: 3).Object);

        var result = await controller.GetMyAlerts();

        var response = result.Result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeAssignableTo<IEnumerable<UserAlertDto>>().Subject
            .ToList();
        response.Should().ContainSingle(alert => alert.Id == 2);
        response.Single().Logs.Should().ContainSingle(log =>
            log.SourceSummary == "NBP");
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
        string? subject = null,
        string? role = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subject ?? AuthId.ToString())
        };
        if (role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var identity = new ClaimsIdentity(claims, "TestAuthentication");

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
