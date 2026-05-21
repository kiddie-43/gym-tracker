using GymTracker.Infrastructure.Observability;

using Grpc.Core;

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

            var statusCode = exception is RpcException rpcException && rpcException.StatusCode == StatusCode.ResourceExhausted
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status500InternalServerError;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode == StatusCodes.Status503ServiceUnavailable
                    ? "Service temporarily unavailable."
                    : "An unexpected error occurred.",
                Detail = statusCode == StatusCodes.Status503ServiceUnavailable
                    ? "A dependent service quota was exceeded. Retry later or contact support if the issue persists."
                    : "Review the server logs with the provided correlation id.",
                Instance = context.Request.Path,
            };

            problemDetails.Extensions["correlationId"] = correlationId;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
