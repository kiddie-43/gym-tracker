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
        var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var isAdminClaim = context.User.FindFirst("custom.admin")?.Value;
        var isAdmin = string.Equals(isAdminClaim, "true", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            context.Items["CurrentUserId"] = userId;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            context.Items["CurrentUserEmail"] = email;
        }

        context.Items["CurrentUserIsAdmin"] = isAdmin;

        await _next(context);
    }
}
