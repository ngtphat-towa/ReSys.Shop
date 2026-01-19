using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using Npgsql;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder, string[]? extraSources = null, string[]? extraMeters = null)
    {
        builder.ConfigureOpenTelemetry(extraSources, extraMeters);

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.AddHttpClient();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder, string[]? extraSources = null, string[]? extraMeters = null)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (extraMeters != null)
                {
                    foreach (var meter in extraMeters) metrics.AddMeter(meter);
                }
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsql()
                    .AddEntityFrameworkCoreInstrumentation();

                if (extraSources != null)
                {
                    foreach (var source in extraSources) tracing.AddSource(source);
                }
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    public static IHostApplicationBuilder AddPostgresHealthCheck(this IHostApplicationBuilder builder, string connectionName)
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName);
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddHealthChecks().AddNpgSql(connectionString, name: connectionName);
        }
        return builder;
    }

    public static IHostApplicationBuilder AddHttpHealthCheck(this IHostApplicationBuilder builder, string name, string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            builder.Services.AddHealthChecks().AddUrlGroup(uri, name);
        }
        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Liveness: Is the process up?
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static IHostApplicationBuilder AddPostgresHealthCheck(this IHostApplicationBuilder builder, string connectionName, string? healthName = null)
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName);
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    connectionString: connectionString,
                    name: healthName ?? $"{connectionName}-db",
                    tags: ["ready", "db", "postgres"]);
        }

        return builder;
    }

    public static IHostApplicationBuilder AddHttpHealthCheck(this IHostApplicationBuilder builder, string serviceName, string url, string[]? tags = null)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            builder.Services.AddHealthChecks()
                .AddUrlGroup(uri, name: $"{serviceName}-check", tags: tags ?? ["ready", "http"]);
        }

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Readiness: All checks including DB and downstream services must pass
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("ready")
            });

            // Liveness: Only the self check must pass
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
