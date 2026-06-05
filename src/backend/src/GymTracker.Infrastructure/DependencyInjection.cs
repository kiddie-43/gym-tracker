using System.Reflection;
using GymTracker.Infrastructure.Admin;
using GymTracker.Infrastructure.Observability;
using GymTracker.Infrastructure.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlConnectionString = configuration.GetSection("Sql").GetValue<string>("ConnectionString");

        services.AddMemoryCache();
        services.AddSingleton<CorrelationIdAccessor>();

        if (!string.IsNullOrWhiteSpace(sqlConnectionString))
        {
            services.AddSingleton(new SqlDocumentStore(sqlConnectionString));
            services.AddDbContext<AdminDbContext>(options =>
                options.UseSqlServer(sqlConnectionString));
        }

        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Repository", StringComparison.Ordinal) &&
                !type.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
