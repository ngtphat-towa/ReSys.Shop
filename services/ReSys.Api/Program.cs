using ReSys.Api;
using ReSys.Core;
using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure;
using ReSys.Infrastructure.Identity;
using ReSys.Infrastructure.ML;

using Serilog;
using Serilog.Events;

// 1. Setup Bootstrap Logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry()
    .CreateBootstrapLogger();

try
{
    Log.Information("ReSys API Starting...");

    var builder = WebApplication.CreateBuilder(args);

    // 2. Use Serilog for Host
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.OpenTelemetry());

    builder.AddServiceDefaults(
        extraSources: [TelemetryConstants.ActivitySource.Name],
        extraMeters: [TelemetryConstants.Meter.Name]);

    builder.AddPostgresHealthCheck("shopdb");

    var mlOptions = builder.Configuration.GetSection(MlOptions.SectionName).Get<MlOptions>();
    if (mlOptions != null)
    {
        builder.AddHttpHealthCheck("ml-service", mlOptions.ServiceUrl + "/health");
    }

    // Register Layers
    builder.Services
        .AddPresentation()
        .AddCore(typeof(ReSys.Api.DependencyInjection).Assembly)
        .AddInfrastructure(builder.Configuration);
    
    // API needs to Validate tokens, not issue them.
    // Ideally, the AUTHORITY URL should come from config. 
    // For local Aspire, it's usually http://identity or similar, but let's assume we get it injected or use a placeholder for now.
    // In Aspire, we'll name the service 'identity'.
    // We read the 'services:identity:http' or similar from Aspire service discovery, 
    // but typically OpenIddict validation needs a raw URL. 
    // For now, we trust the service discovery to handle 'http://identity' if we used YARP or internal DNS.
    // However, OpenIddict Validation requires a valid Discovery Document reachable via HTTP.
    builder.Services.AddIdentityStorage(); 
    builder.Services.AddIdentityValidation(builder.Configuration["services:identity:http"] ?? "http://localhost:5005"); 

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // ---------------------------------------------------------
    // Configure Pipeline (Clean Delegation)
    // ---------------------------------------------------------
    app.UseInfrastructure();
    app.UseCore();
    app.UsePresentation();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }