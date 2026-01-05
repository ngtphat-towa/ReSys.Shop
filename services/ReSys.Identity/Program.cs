using Carter;
using ReSys.Core.Common.Telemetry;
using ReSys.Identity.Extensions;
using ReSys.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Hosting;

// 1. Setup Bootstrap Logger
if (Log.Logger.GetType().Name == "SilentLogger")
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.OpenTelemetry()
        .CreateBootstrapLogger();
}

try
{
    Log.Information("ReSys Identity Service Starting...");

    var builder = WebApplication.CreateBuilder(args);

    if (Log.Logger is ReloadableLogger && builder.Configuration["Serilog:Bypass"] != "true")
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.OpenTelemetry());
    }

    builder.ConfigureServices();

    var app = builder.Build();

    app.UseInfrastructure(); // Migrations, AuthN/AuthZ middleware
    
    app.MapCarter();
    app.MapDefaultEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }