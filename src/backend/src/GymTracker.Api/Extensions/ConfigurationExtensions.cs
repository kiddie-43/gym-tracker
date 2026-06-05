using GymTracker.Api.Options;

namespace GymTracker.Api.Extensions;

public static class ConfigurationExtensions
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>(optional: true);
        }

        builder.Services.Configure<SqlOptions>(builder.Configuration.GetSection(SqlOptions.SectionName));
        builder.Services.Configure<DevelopmentAuthModeOptions>(builder.Configuration.GetSection(DevelopmentAuthModeOptions.SectionName));

        return builder;
    }
}
