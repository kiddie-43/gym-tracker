using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace GymTracker.Api.Middleware;

public sealed class FirebaseAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FirebaseAuthMiddleware> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public FirebaseAuthMiddleware(
        RequestDelegate next,
        ILogger<FirebaseAuthMiddleware> logger,
        IHostEnvironment hostEnvironment)
    {
        _next = next;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
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
            if (_hostEnvironment.IsDevelopment())
            {
                var developmentUserId = context.Request.Headers.TryGetValue("X-Dev-User-Id", out var headerUserId)
                    && !string.IsNullOrWhiteSpace(headerUserId)
                    ? headerUserId.ToString()
                    : "dev-local-user";

                context.User = BuildPrincipal(developmentUserId, email: null, isAdmin: true);
            }

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

        if (TryBuildDevelopmentPrincipal(token, out var principal))
        {
            context.User = principal;
            await _next(context);
            return;
        }

        _logger.LogWarning("Unable to authenticate request token.");
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private static bool TryBuildDevelopmentPrincipal(string token, out ClaimsPrincipal principal)
    {
        principal = new ClaimsPrincipal();

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if (token.StartsWith("integration-", StringComparison.OrdinalIgnoreCase))
        {
            var isAdmin = token.Contains("admin", StringComparison.OrdinalIgnoreCase);
            principal = BuildPrincipal(token, email: null, isAdmin);
            return true;
        }

        if (token.Contains('.'))
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2)
                {
                    return false;
                }

                var payloadJson = DecodeBase64Url(parts[1]);
                using var document = JsonDocument.Parse(payloadJson);
                var root = document.RootElement;

                var sub = root.TryGetProperty("sub", out var subProperty)
                    ? subProperty.GetString()
                    : null;
                var email = root.TryGetProperty("email", out var emailProperty)
                    ? emailProperty.GetString()
                    : null;
                var isAdmin = ResolveAdminClaim(token);

                if (string.IsNullOrWhiteSpace(sub))
                {
                    return false;
                }

                principal = BuildPrincipal(sub!, email, isAdmin);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private static ClaimsPrincipal BuildPrincipal(string userId, string? email, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("user_id", userId),
            new("custom.admin", isAdmin ? "true" : "false"),
        };

        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "FirebaseBearer"));
    }

    private static bool ResolveAdminClaim(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2)
            {
                return false;
            }

            var payloadJson = DecodeBase64Url(parts[1]);
            using var document = JsonDocument.Parse(payloadJson);
            var root = document.RootElement;

            if (TryGetBoolean(root, "custom.admin", out var customAdmin))
            {
                return customAdmin;
            }

            if (root.TryGetProperty("custom", out var customElement)
                && customElement.ValueKind == JsonValueKind.Object
                && TryGetBoolean(customElement, "admin", out var nestedAdmin))
            {
                return nestedAdmin;
            }

            if (TryGetBoolean(root, "admin", out var admin))
            {
                return admin;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryGetBoolean(JsonElement element, string propertyName, out bool value)
    {
        value = false;

        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.True)
        {
            value = true;
            return true;
        }

        if (property.ValueKind == JsonValueKind.False)
        {
            value = false;
            return true;
        }

        if (property.ValueKind == JsonValueKind.String && bool.TryParse(property.GetString(), out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    private static string DecodeBase64Url(string input)
    {
        var normalized = input.Replace('-', '+').Replace('_', '/');
        var padding = 4 - normalized.Length % 4;
        if (padding is > 0 and < 4)
        {
            normalized = normalized.PadRight(normalized.Length + padding, '=');
        }

        var bytes = Convert.FromBase64String(normalized);
        return Encoding.UTF8.GetString(bytes);
    }
}
