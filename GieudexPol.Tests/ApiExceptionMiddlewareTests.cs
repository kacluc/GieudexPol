using System.Net;
using System.Text.Json;
using GieudexPol.API.Middleware;
using GieudexPol.Domain.Auth;
using Microsoft.AspNetCore.Http;

namespace GieudexPol.Tests;

public class ApiExceptionMiddlewareTests
{
    [Fact]
    public async Task InvalidCredentials_ReturnsUnauthorizedProblemDetails()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ApiExceptionMiddleware(
            _ => throw new InvalidCredentialsException());

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var problem = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(
            "Nieprawidłowy adres e-mail lub hasło.",
            problem.RootElement.GetProperty("detail").GetString());
    }
}
