using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RealTest.Application.Common.Exceptions;

namespace RealTest.Api.Middleware;

/// <summary>
/// Single point of translation from exceptions to ProblemDetails. Application-layer exceptions
/// map to their intended status code; anything else is treated as unexpected, logged with full
/// detail server-side, and reported to the caller as a generic 500 with a trace id only.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        if (exception is ValidationAppException validationException)
        {
            logger.LogWarning("Validation failed. TraceId: {TraceId}", traceId);

            var validationProblem = new HttpValidationProblemDetails(validationException.Errors)
            {
                Type = "https://realtest.app/errors/validation",
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest
            };
            validationProblem.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
            return true;
        }

        var (statusCode, type, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "https://realtest.app/errors/not-found", "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "https://realtest.app/errors/conflict", "Conflict"),
            UnauthorizedAppException => (StatusCodes.Status401Unauthorized, "https://realtest.app/errors/unauthorized", "Unauthorized"),
            ForbiddenAppException => (StatusCodes.Status403Forbidden, "https://realtest.app/errors/forbidden", "Forbidden"),
            _ => (StatusCodes.Status500InternalServerError, "https://realtest.app/errors/internal", "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        }
        else
        {
            logger.LogWarning(exception, "{ExceptionType} handled. TraceId: {TraceId}", exception.GetType().Name, traceId);
        }

        var problem = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Please try again or contact support if the problem persists."
                : exception.Message
        };
        problem.Extensions["traceId"] = traceId;

        if (environment.IsDevelopment() && statusCode == StatusCodes.Status500InternalServerError)
        {
            problem.Extensions["exception"] = exception.ToString();
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
