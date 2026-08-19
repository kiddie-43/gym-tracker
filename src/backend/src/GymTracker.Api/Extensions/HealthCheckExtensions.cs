using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GymTracker.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("monthly-plan-module", () => HealthCheckResult.Healthy("Monthly plan module registered."));
        return services;
    }

    public static IEndpointRouteBuilder MapHealthChecksConfiguration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    status = report.Status.ToString(),
                    entries = report.Entries.ToDictionary(
                        entry => entry.Key,
                        entry => new
                        {
                            status = entry.Value.Status.ToString(),
                            description = entry.Value.Description,
                            duration = entry.Value.Duration.TotalMilliseconds,
                            data = entry.Value.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                        })
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        });

        return endpoints;
    }
}
