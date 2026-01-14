using System.Reflection;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Behaviors;
using ReSys.Shared.Telemetry;

using ReSys.Shared.Constants;

namespace ReSys.Core;

public static class CoreModule
{
    public static IServiceCollection AddCore(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services
            .AddCoreCommon(assemblies)
            .AddCoreFeatures();
    }

    public static IServiceCollection AddCoreCommon(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.RegisterModule(ModuleNames.Core, "Common", s =>
        {
            s.AddMediatR(config =>
            {
                foreach (var assembly in assemblies)
                {
                    config.RegisterServicesFromAssembly(assembly);
                }

                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
                config.AddOpenBehavior(typeof(TelemetryBehavior<,>));
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            foreach (var assembly in assemblies)
            {
                s.AddValidatorsFromAssembly(assembly);
            }
        });

        return services;
    }

    public static IServiceCollection AddCoreFeatures(this IServiceCollection services)
    {
        services.RegisterModule(ModuleNames.Core, "Features", s =>
        {
            s.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(CoreModule).Assembly);
            });

            s.AddValidatorsFromAssembly(typeof(CoreModule).Assembly);
        });

        return services;
    }

    public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseCore(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        return app;
    }
}