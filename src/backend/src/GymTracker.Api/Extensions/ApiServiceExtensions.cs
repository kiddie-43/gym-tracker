using GymTracker.Api.Middleware;
using GymTracker.Application.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GymTracker.Api.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddControllers();

        services
            .AddAuthentication(AuthSchemes.FirebaseBearer)
            .AddScheme<AuthenticationSchemeOptions, PassthroughAuthenticationHandler>(
                AuthSchemes.FirebaseBearer,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.DefaultUserPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
            });

            options.AddPolicy(AuthorizationPolicies.AdminOnlyPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("custom.admin", "true");
            });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendDev", policy =>
            {
                policy
                    .SetIsOriginAllowed(origin =>
                    {
                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                        {
                            return false;
                        }

                        var isLocalHost =
                            uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                            || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

                        var isHttpScheme =
                            uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                            || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

                        return isLocalHost && isHttpScheme;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
