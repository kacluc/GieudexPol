using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GieudexPol.Tests
{
    // Jeśli Program nie jest widoczny, upewnij się, że klasa Program w głównym projekcie ma: public partial class Program { }
    public class WalletsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IWalletService> _mockWalletService;

        public WalletsControllerTests(WebApplicationFactory<Program> factory)
        {
            // 1. NAJPIERW inicjalizujemy Mocka, żeby obiekt nie był nullem
            _mockWalletService = new Mock<IWalletService>();

            // 2. DOPIERO POTEM konfigurujemy i wstrzykujemy go do fabryki testowej
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Naprawiono: Użycie standardowej kolekcji 'services' zamiast nieistniejącego SingleWindowDescriptor
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IWalletService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddSingleton(_mockWalletService.Object);
                });
            });
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
                    It.IsAny<decimal>()
                ))
                .Returns(Task.CompletedTask);

            var client = _factory.CreateClient();
            var requestBody = new { fromCurrencyId = 1, amountFrom = 10m, toCurrencyId = 2, amountTo = 5m };

            // Act
            var response = await client.PostAsJsonAsync("api/Wallets/trade?userId=1", requestBody);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ExecuteTrade_InsufficientFunds_ReturnsBadRequest()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Insufficient funds in Wallet.");
            _mockWalletService.Setup(s => s.ExecuteTradeTransactionAsync(
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<int>(), 
                    It.IsAny<decimal>()
                ))
                .ThrowsAsync(expectedException);

            var client = _factory.CreateClient();
            var requestBody = new { fromCurrencyId = 1, amountFrom = 500m, toCurrencyId = 2, amountTo = 1m };

            // Act
            var response = await client.PostAsJsonAsync("api/Wallets/trade?userId=1", requestBody);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ExecuteTrade_InternalError_ReturnsInternalServerError()
        {
            // Arrange
            var internalException = new Exception("Database connection lost.");
            _mockWalletService.Setup(s => s.ExecuteTradeTransactionAsync(
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<int>(), 
                    It.IsAny<decimal>()
                ))
                .ThrowsAsync(internalException);

            var client = _factory.CreateClient();
            var requestBody = new { fromCurrencyId = 1, amountFrom = 10m, toCurrencyId = 2, amountTo = 5m };

            // Act
            var response = await client.PostAsJsonAsync("api/Wallets/trade?userId=1", requestBody);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
