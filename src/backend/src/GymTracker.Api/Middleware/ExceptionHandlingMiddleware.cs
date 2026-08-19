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
            context.Response.Headers["X-Correlation-Id"] = correlationId;

            _logger.LogError(exception, "Unhandled exception for request {Path} with correlation id {CorrelationId}", context.Request.Path, correlationId);

            if (context.Response.HasStarted)
            {
                throw;
            }

            var statusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                InvalidOperationException => StatusCodes.Status409Conflict,
                RpcException rpcException when rpcException.StatusCode == StatusCode.ResourceExhausted => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status500InternalServerError,
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Invalid request.",
                    StatusCodes.Status403Forbidden => "Forbidden.",
                    StatusCodes.Status409Conflict => "Conflict.",
                    StatusCodes.Status503ServiceUnavailable => "Service temporarily unavailable.",
                    _ => "An unexpected error occurred.",
                },
                Detail = statusCode switch
                {
                    StatusCodes.Status400BadRequest => exception.Message,
                    StatusCodes.Status403Forbidden => "You are not allowed to perform this operation.",
                    StatusCodes.Status409Conflict => exception.Message,
                    StatusCodes.Status503ServiceUnavailable => "A dependent service quota was exceeded. Retry later or contact support if the issue persists.",
                    _ => "Review the server logs with the provided correlation id.",
                },
                Instance = context.Request.Path,
            };

            problemDetails.Extensions["correlationId"] = correlationId;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
