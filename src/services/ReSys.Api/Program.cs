using ReSys.Api;
using ReSys.Core;
using ReSys.Shared.Telemetry;
using ReSys.Infrastructure;
using ReSys.Infrastructure.Persistence;
using ReSys.Infrastructure.Ml.Options;

using Microsoft.EntityFrameworkCore;

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

    if (builder.Configuration.GetConnectionString("shopdb") != null)
    {
        builder.AddPostgresHealthCheck("shopdb");
    }

    var mlOptions = builder.Configuration.GetSection(MlOptions.SectionName).Get<MlOptions>();
    if (mlOptions != null && !string.IsNullOrEmpty(mlOptions.ServiceUrl))
    {
        builder.AddHttpHealthCheck("ml-service", mlOptions.ServiceUrl + "/health");
    }

    // Register Layers
    builder.Services
        .AddPresentation()
        .AddCore(typeof(ApiModule).Assembly)
        .AddInfrastructure(builder.Configuration);

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // ---------------------------------------------------------
    // Configure Pipeline (Clean Delegation)
    // ---------------------------------------------------------
    app.UseInfrastructure();
    app.UseCore();
    app.UsePresentation();

    // Apply Migrations
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }

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
