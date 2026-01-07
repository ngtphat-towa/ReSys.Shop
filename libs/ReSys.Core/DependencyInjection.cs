using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Behaviors;
using ReSys.Core.Common.Telemetry;

namespace ReSys.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.RegisterModule("Core", "Application", s =>
        {
            s.AddMediatR(config =>
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

            s.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            foreach (var assembly in assemblies)
            {
                s.AddValidatorsFromAssembly(assembly);
            }
        });

        return services;
    }

    public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseCore(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        return app;
    }
}