using System.Net;
using System.Text.Json;
using mini_mes_be.DTOs;

namespace mini_mes_be.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppValidationException ex)
        {
            logger.LogWarning("Validation failed: {Message}", ex.Message);
            await WriteJsonAsync(context, ex.StatusCode, ex.Message, ex.Errors);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized: {Message}", ex.Message);
            await WriteJsonAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Conflict/Bad request: {Message}", ex.Message);
            await WriteJsonAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteJsonAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteJsonAsync(HttpContext context, HttpStatusCode status, string message, Dictionary<string, string>? errors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var response = ApiResponse.Fail(message, (int)status, errors);
        var body = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(body);
    }
}
