using Carter;
using FluentValidation;
using ReSys.Core;
using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure;
using ReSys.Infrastructure.Identity;
using Serilog;

namespace ReSys.Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // 1. Serilog
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.OpenTelemetry());

        // 2. Service Defaults (Aspire)
        builder.AddServiceDefaults(
            extraSources: [TelemetryConstants.ActivitySource.Name],
            extraMeters: [TelemetryConstants.Meter.Name]);

        // 3. Health Checks
        builder.AddPostgresHealthCheck("shopdb");

        // 4. Infrastructure (DB, etc.)
        builder.Services.AddInfrastructure(builder.Configuration);

        // 5. Core (MediatR, etc.)
        builder.Services.AddCore(typeof(Program).Assembly);

        // 6. Identity Specifics (Storage + Server)
        builder.Services.AddIdentityStorage();
        builder.Services.AddIdentityServer();

        builder.Services.AddAuthorization();

        // 7. Presentation (Carter + Validation)
        builder.Services.AddCarter();
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        return builder;
    }
}
