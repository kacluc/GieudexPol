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

public class TransactionsControllerTests
{
    private static readonly Guid AuthId = new("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GetUserTransactions_WithGuidSubject_UsesMappedApplicationUserId()
    {
        var transactionService = new Mock<ITransactionService>();
        var userRepository = new Mock<IUserRepository>();
        var expectedResult = new PaginatedResult<TransactionDto>
        {
            Items = [],
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        userRepository
            .Setup(repository => repository.GetByAuthIdAsync(AuthId))
            .ReturnsAsync(new User { Id = 7, AuthId = AuthId, Username = "user@example.com" });
        transactionService
            .Setup(service => service.GetUserTransactions(7, 1, 10, null, null, null, null))
            .ReturnsAsync(expectedResult);

        var controller = CreateController(transactionService.Object, userRepository.Object, AuthId.ToString());

        var result = await controller.GetUserTransactions(7);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(expectedResult);
    }

    [Fact]
    public async Task GetUserTransactions_ForDifferentApplicationUser_ReturnsForbidden()
    {
        var transactionService = new Mock<ITransactionService>();
        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(repository => repository.GetByAuthIdAsync(AuthId))
            .ReturnsAsync(new User { Id = 7, AuthId = AuthId, Username = "user@example.com" });

        var controller = CreateController(transactionService.Object, userRepository.Object, AuthId.ToString());

        var result = await controller.GetUserTransactions(8);

        result.Should().BeOfType<ForbidResult>();
        transactionService.Verify(
            service => service.GetUserTransactions(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserTransactions_WithInvalidSubject_ReturnsUnauthorized()
    {
        var transactionService = new Mock<ITransactionService>();
        var userRepository = new Mock<IUserRepository>();
        var controller = CreateController(transactionService.Object, userRepository.Object, "not-a-guid");

        var result = await controller.GetUserTransactions(7);

        result.Should().BeOfType<UnauthorizedResult>();
        userRepository.Verify(
            repository => repository.GetByAuthIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    private static TransactionsController CreateController(
        ITransactionService transactionService,
        IUserRepository userRepository,
        string subject)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, subject)],
            "TestAuthentication");

        return new TransactionsController(transactionService, userRepository)
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
