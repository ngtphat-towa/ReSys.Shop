using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Behaviors;

namespace ReSys.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, params Assembly[] assemblies)
    {
        Serilog.Log.Information("[Core] Initializing Core Layer (MediatR, Behaviors, Validators)...");
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            foreach (var assembly in assemblies)
            {
                config.RegisterServicesFromAssembly(assembly);
            }
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(TelemetryBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        foreach (var assembly in assemblies)
        {
            services.AddValidatorsFromAssembly(assembly);
        }

        return services;
    }

    public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseCore(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        // Placeholder for core pipeline setup if needed in the future
        return app;
    }
}