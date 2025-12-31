using Microsoft.Extensions.Options;
using ReSys.Gateway;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();



// Readiness checks for downstream services
var endpoints = builder.Configuration.GetSection(ServiceEndpoints.SectionName).Get<ServiceEndpoints>();
if (endpoints != null)
{
    builder.AddHttpHealthCheck("api", endpoints.ApiUrl + "health");
    builder.AddHttpHealthCheck("ml", endpoints.MlUrl + "health");
    builder.AddHttpHealthCheck("shop", endpoints.ShopUrl);
    builder.AddHttpHealthCheck("admin", endpoints.AdminUrl);
}

// Options Pattern: Register custom configuration with validation
builder.Services.AddOptions<GatewayOptions>()
    .Bind(builder.Configuration.GetSection(GatewayOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<ServiceEndpoints>()
    .Bind(builder.Configuration.GetSection(ServiceEndpoints.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapDefaultEndpoints();

// Use Options Pattern: Conditionally add custom headers using IOptions
var gatewayOptions = app.Services.GetRequiredService<IOptions<GatewayOptions>>().Value;

if (gatewayOptions.EnableRequestLogging)
{
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Gateway Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next();
    });
}

if (gatewayOptions.CustomHeaders.Count > 0)
{
    app.Use(async (context, next) =>
    {
        foreach (var header in gatewayOptions.CustomHeaders)
        {
            context.Response.Headers.TryAdd(header.Key, header.Value);
        }
        await next();
    });
}

// Expose configuration endpoint (dev only)
if (app.Environment.IsDevelopment())
{
    app.MapGet("/gateway/config", (IOptionsSnapshot<GatewayOptions> options, IOptionsSnapshot<ServiceEndpoints> endpoints) =>
    {
        return Results.Ok(new
        {
            Gateway = options.Value,
            Endpoints = endpoints.Value
        });
    });
}

app.MapReverseProxy();

app.Run();