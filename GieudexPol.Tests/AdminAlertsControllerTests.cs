using System.Reflection;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GieudexPol.API.Controllers;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GieudexPol.Tests;

public class AdminAlertsControllerTests
{
    [Fact]
    public void Controller_RequiresAdminRole()
    {
        var attribute = typeof(AdminAlertsController)
            .GetCustomAttribute<AuthorizeAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Roles.Should().Be(UserRoles.Admin);
        attribute.Roles.Should().NotContain(UserRoles.User);
    }

    [Fact]
    public async Task Evaluate_AsAuthorizedAdminInvocation_ReturnsEvaluationResult()
    {
        var service = new Mock<IAlertEvaluationService>();
        service.Setup(item => item.EvaluateAllActiveAlertsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AlertEvaluationResult
            {
                EvaluatedAlertsCount = 2,
                TriggeredAlertsCount = 1,
                NotificationsCreatedCount = 1
            });
        var controller = new AdminAlertsController(service.Object);

        var response = await controller.Evaluate(null, CancellationToken.None);

        response.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<AlertEvaluationResult>()
            .Which.TriggeredAlertsCount.Should().Be(1);
    }
}

public class AdminAlertsAuthorizationIntegrationTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AdminAlertsAuthorizationIntegrationTests(
        CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Evaluate_OrdinaryUserIsForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRoles.User);

        var response = await client.PostAsJsonAsync(
            "/api/admin/alerts/evaluate",
            new AlertEvaluationRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Evaluate_AdminIsAllowed()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRoles.Admin);

        var response = await client.PostAsJsonAsync(
            "/api/admin/alerts/evaluate",
            new AlertEvaluationRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
