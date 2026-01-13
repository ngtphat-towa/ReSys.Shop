using Microsoft.Extensions.DependencyInjection;


using Serilog;


using System.Diagnostics;


using ReSys.Core.Common.Constants;

namespace ReSys.Core.Common.Telemetry;

public static class TelemetryExtensions
{
    /// <summary>
    /// Logs a standardized module registration event with structured metadata and tracks execution time.
    /// Format: [{Layer}] Registering {Module} module...
    /// </summary>
    public static IServiceCollection RegisterModule(
        this IServiceCollection services, 
        string layer, 
        string module, 
        Action<IServiceCollection> action)
    {
        var logger = Log.ForContext("Layer", layer).ForContext("Module", module);
        using var activity = TelemetryConstants.ActivitySource.StartActivity($"RegisterModule {layer}.{module}");
        
        activity?.SetTag("layer", layer);
        activity?.SetTag("module", module);

        try
        {
            logger.Information(LogTemplates.Bootstrapper.ModuleRegistering, layer, module);
            
            action(services);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            // Note: We're not tracking elapsed time here for the error case in the log message itself 
            // because `logger` doesn't have access to the activity's elapsed time easily in this scope 
            // without casting. For simplicity, we keep the message standard.
            logger.Error(ex, LogTemplates.Bootstrapper.ModuleFailed, layer, module, activity?.Duration.TotalMilliseconds ?? 0);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        return services;
    }
}
