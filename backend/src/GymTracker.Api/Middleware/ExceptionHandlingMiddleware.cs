using GymTracker.Infrastructure.Observability;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, CorrelationIdAccessor correlationIdAccessor)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var headerValue)
                ? headerValue.ToString()
                : context.TraceIdentifier;

            correlationIdAccessor.Set(correlationId);

            _logger.LogError(exception, "Unhandled exception for request {Path} with correlation id {CorrelationId}", context.Request.Path, correlationId);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Review the server logs with the provided correlation id.",
                Instance = context.Request.Path,
            };

            problemDetails.Extensions["correlationId"] = correlationId;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
