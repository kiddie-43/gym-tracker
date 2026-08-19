using System.Reflection;
using GymTracker.Application.MonthlyPlan;
using Microsoft.Extensions.DependencyInjection;

namespace GymTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<MonthlyPlanService>();

        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Service", StringComparison.Ordinal) &&
                !type.IsAbstract))
                .AsSelf()
                .WithScopedLifetime()
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Handlers", StringComparison.Ordinal) &&
                !type.IsAbstract))
                .AsSelf()
                .WithScopedLifetime()
                 .AddClasses(classes => classes.Where(type =>
        type.Name.EndsWith("Definition", StringComparison.Ordinal) &&
        !type.IsAbstract))
        .AsSelf()
        .WithScopedLifetime()
                );


        return services;
    }
}
