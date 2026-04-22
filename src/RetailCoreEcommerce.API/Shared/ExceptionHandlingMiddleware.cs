using System.Net;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using RetailCoreEcommerce.Contracts.Shared;

namespace RetailCoreEcommerce.API.Shared;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var statusCode = HttpStatusCode.InternalServerError;
        var errorResponse =
            Result.Failure(new Error("Server.Error", "An error occurred while processing your request"));

        switch (exception)
        {
            case SecurityTokenException:
                statusCode = HttpStatusCode.Unauthorized;
                errorResponse =
                    Result.Failure(new Error("Auth.InvalidToken", "The provided token is invalid or expired"));
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
