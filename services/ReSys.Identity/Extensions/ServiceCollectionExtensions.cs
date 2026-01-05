using Carter;
using FluentValidation;
using ReSys.Core;
using ReSys.Core.Common.Mailing;
using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure;
using ReSys.Infrastructure.Identity;

namespace ReSys.Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // 1. Service Defaults (Aspire)
        builder.AddServiceDefaults(
            extraSources: [TelemetryConstants.ActivitySource.Name],
            extraMeters: [TelemetryConstants.Meter.Name]);

        // 2. Health Checks
        builder.AddPostgresHealthCheck("shopdb");

        // 3. Infrastructure (DB, etc.)
        builder.Services.AddInfrastructure(builder.Configuration);

        // 4. Core (MediatR, etc.)
        builder.Services.AddCore(typeof(Program).Assembly);
        builder.Services.AddTransient<IMailService, FakeMailService>();

        // 5. Identity Specifics (Storage + Server)
        builder.Services.AddIdentityStorage();
        builder.Services.AddIdentityServer();

        builder.Services.AddAuthorization();

        // 6. Presentation (Carter + Validation)
        builder.Services.AddCarter();
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        return builder;
    }
}
