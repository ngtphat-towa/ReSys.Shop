using Carter;
using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure;
using ReSys.Infrastructure.Identity;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry()
    .CreateBootstrapLogger();

try
{
    Log.Information("ReSys Identity Service Starting...");

    var builder = WebApplication.CreateBuilder(args);

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

    // Add Infrastructure (DB, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add Identity Specifics (Storage + Server)
    builder.Services.AddIdentityStorage();
    builder.Services.AddIdentityServer();

    builder.Services.AddCarter();

    var app = builder.Build();

    app.UseInfrastructure(); // Migrations, AuthN/AuthZ middleware
    
    app.MapCarter();
    app.MapDefaultEndpoints();

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