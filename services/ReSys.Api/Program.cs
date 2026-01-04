using ReSys.Api;
using ReSys.Core;
using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure;
using ReSys.Infrastructure.AI;
using Serilog;
using Serilog.Events;

// 1. Setup Bootstrap Logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry() // Added for Aspire/OTel
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

    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.MapDefaultEndpoints();

    // Configure Pipeline
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