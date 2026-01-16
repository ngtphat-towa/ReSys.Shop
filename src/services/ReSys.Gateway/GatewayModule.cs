using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using ReSys.Shared.Telemetry;

using Serilog;

using ReSys.Shared.Constants;

namespace ReSys.Gateway;

public static class GatewayModule
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule(ModuleNames.Infrastructure, "Gateway", s =>
        {
            // Options
            s.AddOptions<GatewayOptions>()
                .Bind(configuration.GetSection(GatewayOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            s.AddOptions<ServiceEndpoints>()
                .Bind(configuration.GetSection(ServiceEndpoints.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // CORS
            s.AddCors(options =>
            {
                var gatewayOptions = configuration.GetSection(GatewayOptions.SectionName).Get<GatewayOptions>();
                options.AddDefaultPolicy(policy =>
                {
                    if (gatewayOptions?.AllowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(gatewayOptions.AllowedOrigins);
                    }
                    else
                    {
                        policy.AllowAnyOrigin();
                    }

                    policy.AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // YARP
            s.AddReverseProxy()
                .LoadFromConfig(configuration.GetSection(GatewayOptions.ReverseProxySection))
                .AddServiceDiscoveryDestinationResolver();

            // Rate Limiting
            var gatewayOptions = configuration.GetSection(GatewayOptions.SectionName).Get<GatewayOptions>();
            if (gatewayOptions?.EnableRateLimiting == true)
            {
                s.AddRateLimiter(options =>
                {
                    options.AddFixedWindowLimiter("fixed", opt =>
                    {
                        opt.Window = TimeSpan.FromMinutes(1);
                        opt.PermitLimit = gatewayOptions.MaxRequestsPerMinute;
                    });
                });
            }
        });

        return services;
    }

    public static WebApplication UseGateway(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<GatewayOptions>>().Value;

        // 1. Diagnostics
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "GW {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        // 2. Health Checks
        app.MapDefaultEndpoints();

        // 3. Custom Headers Middleware
        if (options.CustomHeaders.Count > 0)
        {
            app.Use(async (context, next) =>
            {
                foreach (var header in options.CustomHeaders)
                {
                    context.Response.Headers.TryAdd(header.Key, header.Value);
                }
                await next();
            });
        }

        // 4. Rate Limiting (if enabled)
        if (options.EnableRateLimiting)
        {
            app.UseRateLimiter();
        }

        // 5. Security
        app.UseCors();

        // 6. Reverse Proxy Execution
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
