using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;

namespace GieudexPol.Tests;

public class WhaleRankingControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public WhaleRankingControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ReturnsFrontendCompatibleRankingDto()
    {
        var rankingService = new Mock<IWhaleRankingService>();
        rankingService
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(
            [
                new WhaleRankingDto
                {
                    Id = 1,
                    UserId = 7,
                    Username = "dev@gieudexpol.local",
                    TotalPortfolioValue = 12345.67m,
                    Rank = 1,
                    LastUpdated = new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc)
                }
            ]);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.Single(
                    service => service.ServiceType == typeof(IWhaleRankingService));
                services.Remove(descriptor);
                services.AddSingleton(rankingService.Object);
            });
        }).CreateClient();

        var response = await client.GetAsync("/api/whale-ranking");
        var rankings = await response.Content.ReadFromJsonAsync<List<WhaleRankingDto>>();

        Assert.True(response.IsSuccessStatusCode);
        var ranking = Assert.Single(rankings!);
        Assert.Equal("dev@gieudexpol.local", ranking.Username);
        Assert.Equal(12345.67m, ranking.TotalPortfolioValue);
    }
}
