using ReSys.Gateway;

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
    Log.Information("ReSys Gateway Starting...");

    var builder = WebApplication.CreateBuilder(args);

    // 2. Use Serilog for Host
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.OpenTelemetry());

    builder.AddServiceDefaults();

    // 3. Register Gateway Logic
    builder.Services.AddGatewayServices(builder.Configuration);

    var app = builder.Build();

    // 4. Configure Pipeline
    app.UseGateway();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
