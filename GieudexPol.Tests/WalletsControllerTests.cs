using GieudexPol.Application.Interfaces;
using GieudexPol.Application.DTOs;

using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using System;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GieudexPol.Infrastructure.Data;
using GieudexPol.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using GieudexPol.API; // Add this using directive

namespace GieudexPol.Tests
{
    public class WalletsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly Mock<IWalletService> _mockWalletService;

        public WalletsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _mockWalletService = new Mock<IWalletService>();
            _factory = factory;
        }

        [Fact]
        public async Task ExecuteTrade_SuccessfulTrade_ReturnsOk()
        {
            // Arrange
            _mockWalletService.Setup(s => s.ExecuteTradeTransactionAsync(
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<int>(), 
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new TradeExecutionResultDto
                {
                    AmountTo = 2.35m,
                    FromCurrency = "PLN",
                    ToCurrency = "EUR",
                    FromRateToPln = 1m,
                    ToRateToPln = 4.25m,
                    SellRateSource = "PLN",
                    BuyRateSource = "ECB",
                    EffectiveDate = DateTime.Today
                });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IWalletService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddSingleton(_mockWalletService.Object);
                });
            }).CreateClient();

            var requestBody = new { fromCurrencyId = 1, amountFrom = 10m, toCurrencyId = 2 };

            // Act
            var response = await client.PostAsJsonAsync("api/Wallets/trade?userId=1", requestBody);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(json.RootElement.GetProperty("success").GetBoolean());
            Assert.Equal(2.35m, json.RootElement.GetProperty("amountTo").GetDecimal());
            Assert.Equal("ECB", json.RootElement.GetProperty("buyRateSource").GetString());
        }

        [Fact]
        public async Task ExecuteTrade_CannotUseAnotherUserId()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.FirstOrDefault(d =>
                        d.ServiceType == typeof(IWalletService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddSingleton(_mockWalletService.Object);
                });
            }).CreateClient();

            var response = await client.PostAsJsonAsync(
                "api/Wallets/trade?userId=2",
                new { fromCurrencyId = 1, amountFrom = 10m, toCurrencyId = 2 });

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
            _mockWalletService.Verify(service =>
                service.ExecuteTradeTransactionAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task PreviewExchange_UsesAuthenticatedUser_WithoutExecutingTrade()
        {
            _mockWalletService.Setup(service => service.PreviewTradeAsync(
                    1,
                    1,
                    3900m,
                    2,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ExchangePreviewResultDto
                {
                    FromCurrencyCode = "PLN",
                    ToCurrencyCode = "USD",
                    InputAmount = 3900m,
                    EstimatedOutputAmount = 1000m,
                    Rate = 1m / 3.9m,
                    FeeAmount = 19.5m,
                    FeeCurrencyCode = "PLN",
                    TotalDebitAmount = 3919.5m,
                    RateDate = DateTime.Today,
                    IsPreview = true
                });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.FirstOrDefault(d =>
                        d.ServiceType == typeof(IWalletService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton(_mockWalletService.Object);
                });
            }).CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/wallet/exchange/preview",
                new ExchangePreviewRequestDto
                {
                    FromCurrencyId = 1,
                    ToCurrencyId = 2,
                    Amount = 3900m
                });

            Assert.True(response.IsSuccessStatusCode);
            var result = await response.Content.ReadFromJsonAsync<ExchangePreviewResultDto>();
            Assert.NotNull(result);
            Assert.True(result.IsPreview);
            Assert.Equal(1000m, result.EstimatedOutputAmount);

            _mockWalletService.Verify(service => service.PreviewTradeAsync(
                    1,
                    1,
                    3900m,
                    2,
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _mockWalletService.Verify(service => service.ExecuteTradeTransactionAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme,
                _ => { });

            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Mock other services as needed for WalletsController and its dependencies within the test host
            // This ensures that the application context in tests uses mocked versions of these services.
            var transactionServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITransactionService));
            if (transactionServiceDescriptor != null) services.Remove(transactionServiceDescriptor);
            services.AddTransient(sp => new Mock<ITransactionService>().Object);

            var currencyServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyService));
            if (currencyServiceDescriptor != null) services.Remove(currencyServiceDescriptor);
            services.AddTransient(sp => new Mock<ICurrencyService>().Object);

            var exchangeRateServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IExchangeRateService));
            if (exchangeRateServiceDescriptor != null) services.Remove(exchangeRateServiceDescriptor);
            services.AddTransient(sp => new Mock<IExchangeRateService>().Object);

            var userServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUserService));
            if (userServiceDescriptor != null) services.Remove(userServiceDescriptor);
            services.AddTransient(sp => new Mock<IUserService>().Object);

            var transactionFeeRepositoryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITransactionFeeRepository));
            if (transactionFeeRepositoryDescriptor != null) services.Remove(transactionFeeRepositoryDescriptor);
            services.AddTransient(sp => new Mock<ITransactionFeeRepository>().Object);

            var walletRepositoryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IWalletRepository));
            if (walletRepositoryDescriptor != null) services.Remove(walletRepositoryDescriptor);
            services.AddTransient(sp => new Mock<IWalletRepository>().Object);

            var userRepositoryDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IUserRepository));
            if (userRepositoryDescriptor != null) services.Remove(userRepositoryDescriptor);
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.GetByAuthIdAsync(
                    TestAuthHandler.AuthId))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    AuthId = TestAuthHandler.AuthId,
                    Username = "test@local",
                    Role = "User"
                });
            services.AddSingleton(userRepository.Object);
        });
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "Test";
    public static readonly Guid AuthId =
        new("12345678-1234-1234-1234-123456789012");

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, AuthId.ToString())
        };
        if (Request.Headers.TryGetValue("X-Test-Role", out var role) &&
            !string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
