using System.Reflection;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Behaviors;
using ReSys.Shared.Telemetry;

using ReSys.Shared.Constants;
using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Common.Mappings;
using ReSys.Core.Features.Catalog.Taxonomies.Services;

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
            s.AddMappings(assemblies);

            s.AddMediatR(config =>
            {
                foreach (var assembly in assemblies)
                {
                    config.RegisterServicesFromAssembly(assembly);
                }

                config.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
                config.AddOpenBehavior(typeof(TaxonWorkBehavior<,>));
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
                config.AddOpenBehavior(typeof(TelemetryBehavior<,>));
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            foreach (var assembly in assemblies)
            {
                s.AddValidatorsFromAssembly(assembly);
            }

            s.AddHttpContextAccessor();
            s.AddScoped<IUserContext, UserContext>();
            s.AddScoped<ITaxonWorkRegistry, TaxonWorkRegistry>();
            
            // Taxonomy Services
            s.AddScoped<Features.Catalog.Taxonomies.Services.ITaxonHierarchyService, Features.Catalog.Taxonomies.Services.TaxonHierarchyService>();
            s.AddScoped<Features.Catalog.Taxonomies.Services.ITaxonRegenerationService, Features.Catalog.Taxonomies.Services.TaxonRegenerationService>();
        });

        return services;
    }

    public static IServiceCollection AddCoreFeatures(this IServiceCollection services)
    {
        services.RegisterModule(ModuleNames.Core, "Features", s =>
        {
            // Scan Core assembly for features
            s.AddMappings(typeof(CoreModule).Assembly);

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