using FluentValidation;
using TransactionApi.Domain.Exceptions;

namespace TransactionApi.Api.Middleware;

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
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteErrorResponse(context, "ValidationError",
                ex.Errors.Select(e => new ApiError(e.PropertyName, e.ErrorCode, e.ErrorMessage)));
        }
        catch (AccountNotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteErrorResponse(context, "NotFound",
                [new ApiError(null, "ACCOUNT_NOT_FOUND", ex.Message)]);
        }
        catch (InsufficientFundsException ex)
        {
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await WriteErrorResponse(context, "BusinessError",
                [new ApiError(null, "INSUFFICIENT_FUNDS", ex.Message)]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteErrorResponse(context, "InternalServerError",
                [new ApiError(null, "SERVER_ERROR", "An unexpected error occurred.")]);
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context, string type, IEnumerable<ApiError> errors)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { type, errors });
    }
}

public record ApiError(string? Field, string? Code, string Message);
