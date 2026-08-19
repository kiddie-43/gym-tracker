using GymTracker.Api.Middleware;
using GymTracker.Api.Options;

namespace GymTracker.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        var developmentAuthOptions = app.Configuration
            .GetSection(DevelopmentAuthModeOptions.SectionName)
            .Get<DevelopmentAuthModeOptions>()
            ?? new DevelopmentAuthModeOptions();

        if (app.Environment.IsDevelopment() && developmentAuthOptions.Enabled)
        {
            app.Logger.LogWarning(
                "Development auth mode is ENABLED. Simulated identity '{UserId}' (admin={IsAdmin}) will be used when Authorization header is missing.",
                developmentAuthOptions.UserId,
                developmentAuthOptions.IsAdmin);
        }

        if (!app.Environment.IsDevelopment() && developmentAuthOptions.Enabled)
        {
            app.Logger.LogError(
                "Development auth mode was configured outside Development and has been ignored. Environment={EnvironmentName}",
                app.Environment.EnvironmentName);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors("FrontendDev");

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseMiddleware<CurrentUserContextMiddleware>();
        app.UseAuthorization();

        app.MapHealthChecksConfiguration();
        app.MapControllers().RequireCors("FrontendDev");

        return app;
    }
}
