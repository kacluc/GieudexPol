using System.Reflection;
using FluentAssertions;
using GieudexPol.API.Controllers;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GieudexPol.Tests;

public class AdminUsersControllerTests
{
    [Fact]
    public void Controller_RequiresAdminRole()
    {
        var authorizeAttribute = typeof(AdminUsersController)
            .GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Roles.Should().Be(UserRoles.Admin);
    }

    [Fact]
    public void Controller_DoesNotGrantAccessToOrdinaryUserRole()
    {
        var authorizeAttribute = typeof(AdminUsersController)
            .GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute!.Roles.Should().NotContain(UserRoles.User);
    }

    [Fact]
    public async Task GetUsers_ReturnsListForAuthorizedControllerInvocation()
    {
        var service = new Mock<IAdminUserService>();
        service.Setup(item => item.GetUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new AdminUserDto
                {
                    Id = 1,
                    Email = "admin@example.com",
                    Username = "admin@example.com",
                    Role = UserRoles.Admin
                }
            });
        var controller = new AdminUsersController(service.Object);

        var result = await controller.GetUsers(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<AdminUserDto>>()
            .Which.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedUser()
    {
        var request = new CreateAdminUserDto
        {
            Email = "user@example.com",
            Password = "Password123!",
            Role = UserRoles.User
        };
        var service = new Mock<IAdminUserService>();
        service.Setup(item => item.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminUserDto
            {
                Id = 7,
                Email = request.Email,
                Username = request.Email,
                Role = request.Role
            });
        var controller = new AdminUsersController(service.Object);

        var result = await controller.CreateUser(request, CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.RouteValues!["id"].Should().Be(7);
    }

    [Fact]
    public async Task UpdateRole_ReturnsBadRequestForInvalidRole()
    {
        var service = new Mock<IAdminUserService>();
        service.Setup(item => item.UpdateRoleAsync(
                1,
                "Invalid",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Rola musi miec wartosc Admin albo User."));
        var controller = new AdminUsersController(service.Object);

        var result = await controller.UpdateRole(
            1,
            new UpdateUserRoleDto { Role = "Invalid" },
            CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_DelegatesToSecureService()
    {
        var service = new Mock<IAdminUserService>();
        service.Setup(item => item.ResetPasswordAsync(
                2,
                "NewPassword123!",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new AdminUsersController(service.Object);

        var result = await controller.ResetPassword(
            2,
            new ResetUserPasswordDto { NewPassword = "NewPassword123!" },
            CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        service.Verify(item => item.ResetPasswordAsync(
            2,
            "NewPassword123!",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
