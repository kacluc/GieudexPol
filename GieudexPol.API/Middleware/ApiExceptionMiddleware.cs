using GieudexPol.Domain.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GieudexPol.API.Middleware
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InvalidCredentialsException)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    "Nieudane logowanie",
                    "Nieprawidłowy adres e-mail lub hasło.");
            }
            catch (UserAlreadyExistsException ex)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    "Nie udało się utworzyć konta",
                    ex.Message);
            }
            catch (UserNotFoundException ex)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    "Nie znaleziono użytkownika",
                    ex.Message);
            }
        }

        private static async Task WriteProblemAsync(
            HttpContext context,
            int statusCode,
            string title,
            string detail)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
