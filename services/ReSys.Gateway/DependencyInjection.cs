using Microsoft.Extensions.Options;
using ReSys.Core.Common.Telemetry;
using Serilog;

namespace ReSys.Gateway;

public static class DependencyInjection
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Infrastructure", "Gateway");

        // Options
        services.AddOptions<GatewayOptions>()
            .Bind(configuration.GetSection(GatewayOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ServiceEndpoints>()
            .Bind(configuration.GetSection(ServiceEndpoints.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // YARP
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddServiceDiscoveryDestinationResolver();

        return services;
    }

    public static WebApplication UseGateway(this WebApplication app)
    {
        // 1. Diagnostics
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "GW {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        // 2. Health Checks
        app.MapDefaultEndpoints();

        // 3. Security
        app.UseCors();

        // 4. Reverse Proxy Execution
        app.MapReverseProxy();

        // Developer endpoint
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/gateway/config", (IOptions<GatewayOptions> options, IOptions<ServiceEndpoints> endpoints) =>
            {
                return Results.Ok(new
                {
                    Gateway = options.Value,
                    Endpoints = endpoints.Value
                });
            });
        }

        return app;
    }
}
