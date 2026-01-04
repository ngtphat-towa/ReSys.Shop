using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ReSys.Core.Common.Telemetry;

public static class TelemetryExtensions
{
    /// <summary>
    /// Logs a standardized module registration event with structured metadata.
    /// Format: [{Layer}] Registering {Module} module...
    /// </summary>
    public static IServiceCollection RegisterModule(this IServiceCollection services, string layer, string module)
    {
        Log.Information("[{Layer}] Registering {Module} module...", layer, module);
        
        // You could also add tags to a startup activity here if one was active
        return services;
    }
}
