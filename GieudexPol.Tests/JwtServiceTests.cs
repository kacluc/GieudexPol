using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using GieudexPol.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;

namespace GieudexPol.Tests;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_IncludesAdminRoleClaim()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-that-is-long-enough-for-hmac-sha256",
                ["Jwt:Issuer"] = "GieudexPol.Tests",
                ["Jwt:Audience"] = "GieudexPol.Tests",
                ["Jwt:ExpireMinutes"] = "30"
            })
            .Build();
        var service = new JwtService(configuration);

        var token = service.GenerateToken(
            Guid.NewGuid().ToString(),
            "admin@example.com",
            "Admin");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(claim =>
            (claim.Type == ClaimTypes.Role || claim.Type == "role") &&
            claim.Value == "Admin");
    }
}
