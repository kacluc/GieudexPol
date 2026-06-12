using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthUser = GieudexPol.Domain.Auth.User;
using UserEntity = GieudexPol.Domain.Entities.User;

namespace GieudexPol.Tests;

public class AdminUserServiceTests
{
    [Fact]
    public async Task GetUsersAsync_ReturnsUsersWithoutPasswordHashes()
    {
        await using var context = CreateContext();
        context.Users.Add(new UserEntity
        {
            AuthId = Guid.NewGuid(),
            Username = "admin@example.com",
            DisplayName = "Admin",
            PasswordHash = "secret-hash",
            Role = UserRoles.Admin
        });
        await context.SaveChangesAsync();
        var service = new AdminUserService(context);

        var result = await service.GetUsersAsync();

        result.Should().ContainSingle();
        result[0].Email.Should().Be("admin@example.com");
        typeof(AdminUserDto).GetProperty("PasswordHash").Should().BeNull();
        typeof(AdminUserDto).GetProperty("Password").Should().BeNull();
    }

    [Fact]
    public async Task GetUsersAsync_NormalizesStoredRoleForRoleSelector()
    {
        await using var context = CreateContext();
        context.Users.Add(new UserEntity
        {
            AuthId = Guid.NewGuid(),
            Username = "admin@example.com",
            PasswordHash = "hash",
            Role = "admin"
        });
        await context.SaveChangesAsync();
        var service = new AdminUserService(context);

        var result = await service.GetUsersAsync();

        result.Single().Role.Should().Be(UserRoles.Admin);
    }

    [Fact]
    public async Task CreateUserAsync_CreatesUserWithHashedPassword()
    {
        await using var context = CreateContext();
        var service = new AdminUserService(context);
        var request = new CreateAdminUserDto
        {
            Email = "new.user@example.com",
            Password = "Password123!",
            Role = UserRoles.User
        };

        var result = await service.CreateUserAsync(request);

        result.Role.Should().Be(UserRoles.User);
        var storedUser = await context.Users.SingleAsync();
        storedUser.PasswordHash.Should().NotBe(request.Password);
        var authUser = new AuthUser(
            storedUser.AuthId,
            storedUser.Username,
            storedUser.PasswordHash,
            storedUser.Id,
            storedUser.Role);
        var verification = new PasswordHasher<AuthUser>().VerifyHashedPassword(
            authUser,
            storedUser.PasswordHash,
            request.Password);
        verification.Should().BeOneOf(
            PasswordVerificationResult.Success,
            PasswordVerificationResult.SuccessRehashNeeded);
    }

    [Fact]
    public async Task UpdateRoleAsync_ChangesRole()
    {
        await using var context = CreateContext();
        var user = new UserEntity
        {
            AuthId = Guid.NewGuid(),
            Username = "user@example.com",
            PasswordHash = "hash",
            Role = UserRoles.User
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = new AdminUserService(context);

        var result = await service.UpdateRoleAsync(user.Id, "admin");

        result.Should().NotBeNull();
        result!.Role.Should().Be(UserRoles.Admin);
        (await context.Users.FindAsync(user.Id))!.Role.Should().Be(UserRoles.Admin);
    }

    [Fact]
    public async Task UpdateRoleAsync_RejectsInvalidRole()
    {
        await using var context = CreateContext();
        var service = new AdminUserService(context);

        var action = () => service.UpdateRoleAsync(1, "SuperAdmin");

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Admin albo User*");
    }

    [Fact]
    public async Task UpdateRoleAsync_DoesNotDemoteLastAdministrator()
    {
        await using var context = CreateContext();
        var admin = new UserEntity
        {
            AuthId = Guid.NewGuid(),
            Username = "admin@example.com",
            PasswordHash = "hash",
            Role = UserRoles.Admin
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();
        var service = new AdminUserService(context);

        var action = () => service.UpdateRoleAsync(admin.Id, UserRoles.User);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ostatniemu administratorowi*");
    }

    [Fact]
    public async Task ResetPasswordAsync_ReplacesHashAndNeverStoresPlainTextPassword()
    {
        await using var context = CreateContext();
        var user = new UserEntity
        {
            AuthId = Guid.NewGuid(),
            Username = "user@example.com",
            PasswordHash = "old-hash",
            Role = UserRoles.User
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = new AdminUserService(context);
        const string newPassword = "NewPassword123!";

        var updated = await service.ResetPasswordAsync(user.Id, newPassword);

        updated.Should().BeTrue();
        var storedUser = await context.Users.FindAsync(user.Id);
        storedUser!.PasswordHash.Should().NotBe(newPassword);
        storedUser.PasswordHash.Should().NotBe("old-hash");
        var authUser = new AuthUser(
            storedUser.AuthId,
            storedUser.Username,
            storedUser.PasswordHash,
            storedUser.Id,
            storedUser.Role);
        new PasswordHasher<AuthUser>().VerifyHashedPassword(
                authUser,
                storedUser.PasswordHash,
                newPassword)
            .Should()
            .BeOneOf(
                PasswordVerificationResult.Success,
                PasswordVerificationResult.SuccessRehashNeeded);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
