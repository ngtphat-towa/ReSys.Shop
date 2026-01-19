using System.Reflection;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Behaviors;
using ReSys.Shared.Telemetry;

using ReSys.Shared.Constants;
using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Common.Mappings;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

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
            s.AddScoped<ITaxonEvaluator, TaxonEvaluator>();
            s.AddScoped<ITaxonHierarchyService, TaxonHierarchyService>();
            s.AddScoped<ITaxonRegenerationService, TaxonRegenerationService>();

            // Inventory Services
            s.AddScoped<FulfillmentStrategyFactory>();
            s.AddScoped<GreedyFulfillmentStrategy>();
            s.AddScoped<IFulfillmentPlanner, FulfillmentPlanner>();
            s.AddScoped<IInventoryProjectionService, InventoryProjectionService>();
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
