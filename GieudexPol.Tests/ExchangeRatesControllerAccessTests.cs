using System.Security.Claims;
using FluentAssertions;
using GieudexPol.API.Controllers;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GieudexPol.Tests;

public class ExchangeRatesControllerAccessTests
{
    [Fact]
    public async Task GetLatestRates_AdminCanAccessMockBankB()
    {
        var exchangeRateService = new Mock<IExchangeRateService>();
        exchangeRateService
            .Setup(service => service.GetLatestRatesAsync("MOCK_BANK_B", null))
            .ReturnsAsync(Array.Empty<ExchangeRateTableRowDto>());
        var controller = CreateController(
            exchangeRateService.Object,
            UserRoles.Admin);

        var result = await controller.GetLatestRates("MOCK_BANK_B");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLatestRates_OrdinaryUserCannotAccessMockBankB()
    {
        var exchangeRateService = new Mock<IExchangeRateService>();
        var controller = CreateController(
            exchangeRateService.Object,
            UserRoles.User);

        var result = await controller.GetLatestRates("MOCK_BANK_B");

        result.Should().BeOfType<ForbidResult>();
        exchangeRateService.Verify(
            service => service.GetLatestRatesAsync(
                It.IsAny<string>(),
                It.IsAny<string?>()),
            Times.Never);
    }

    private static ExchangeRatesController CreateController(
        IExchangeRateService exchangeRateService,
        string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        return new ExchangeRatesController(
            exchangeRateService,
            Mock.Of<IExchangeRateSyncService>())
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
