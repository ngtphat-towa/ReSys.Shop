using System.Diagnostics;
using System.Reflection;

using Mapster;
using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using ReSys.Shared.Constants;

using Serilog;

namespace ReSys.Core.Common.Mappings;

/// <summary>
/// Extension methods for configuring Mapster object mapping.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Registers Mapster for high-performance object mapping with global configuration.
    /// Configuration is scanned from the provided assemblies and registered as singleton.
    /// </summary>
    public static IServiceCollection AddMappings(this IServiceCollection services, params Assembly[] assemblies)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Use global settings
            var config = TypeAdapterConfig.GlobalSettings;

            // Scan: Assemblies for mapping configurations (IRegister implementations)
            foreach (var assembly in assemblies)
            {
                config.Scan(assembly);
            }

            // Register: Global configuration as singleton
            services.AddSingleton(config);

            // Register: Mapper service as scoped
            services.AddScoped<IMapper, ServiceMapper>();

            stopwatch.Stop();

            Log.Information(
                LogTemplates.Bootstrapper.ConfigLoaded,
                "Mapster",
                string.Join(", ", assemblies.Select(a => a.GetName().Name)),
                config.RuleMap.Count,
                stopwatch.Elapsed.TotalMilliseconds);

            Log.Debug(LogTemplates.Bootstrapper.ServiceRegistered, "TypeAdapterConfig", "Singleton");
            Log.Debug(LogTemplates.Bootstrapper.ServiceRegistered, "IMapper", "Scoped");

            return services;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(
                LogTemplates.Bootstrapper.ConfigFailed,
                "Mapster",
                stopwatch.Elapsed.TotalMilliseconds,
                ex.Message);
            throw;
        }
    }
}