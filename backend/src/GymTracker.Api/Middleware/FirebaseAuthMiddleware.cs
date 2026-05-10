using System.Security.Claims;

namespace GymTracker.Api.Middleware;

public sealed class FirebaseAuthMiddleware
{
    private readonly RequestDelegate _next;

    public FirebaseAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/health") || context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            await _next(context);
            return;
        }

        var headerValue = authorizationHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var token = headerValue["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, token),
            new("user_id", token),
        };

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "FirebaseBearer"));

        await _next(context);
    }
}
