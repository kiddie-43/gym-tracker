namespace GymTracker.Api.Middleware;

public sealed class CurrentUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirst("user_id")?.Value
            ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            context.Items["CurrentUserId"] = userId;
        }

        await _next(context);
    }
}
