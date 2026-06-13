using System.Reflection;
using FluentAssertions;
using GieudexPol.API.Controllers;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GieudexPol.Tests;

public class AdminSystemAccountsControllerTests
{
    [Fact]
    public void Controller_RequiresAdminRole()
    {
        var authorizeAttribute = typeof(AdminSystemAccountsController)
            .GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Roles.Should().Be(UserRoles.Admin);
    }

    [Fact]
    public async Task GetAccounts_ReturnsServiceResult()
    {
        var accounts = new[]
        {
            new AdminSystemAccountDto
            {
                UserId = 10,
                Username = "system_ecb",
                AccountType = "RateSourceSystem"
            }
        };
        var service = new Mock<IAdminSystemAccountService>();
        service.Setup(item => item.GetAccountsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);
        var controller = new AdminSystemAccountsController(service.Object);

        var result = await controller.GetAccounts(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(accounts);
    }
}
