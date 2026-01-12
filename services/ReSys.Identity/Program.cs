using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Identity.Domain;
using ReSys.Identity.Persistence;
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
    Log.Information("ReSys Identity Starting...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.OpenTelemetry());

    builder.AddServiceDefaults();

    // 1. Database
    builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("identitydb"));
        options.UseOpenIddict();
    });

    // 2. Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<AppIdentityDbContext>()
        .AddDefaultTokenProviders();

    // 3. OpenIddict
    builder.Services.AddOpenIddict()
        .AddCore(options =>
        {
            options.UseEntityFrameworkCore()
                   .UseDbContext<AppIdentityDbContext>();
        })
        .AddServer(options =>
        {
            options.SetTokenEndpointUris("/connect/token");

            // Enable flows
            options.AllowPasswordFlow();
            options.AllowRefreshTokenFlow();

            // Accept anonymous clients (for simple password flow without client_id/secret)
            // Or enforce client_id. For "Shop", anonymous is often okay if using ROPC, 
            // but explicit client_id is better.
            options.AcceptAnonymousClients();

            // Development keys (Replace with real certs in prod)
            options.AddDevelopmentEncryptionCertificate()
                   .AddDevelopmentSigningCertificate();

            options.UseAspNetCore()
                   .EnableTokenEndpointPassthrough()
                   .DisableTransportSecurityRequirement();
        })
        .AddValidation(options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
        });

    builder.Services.AddCarter();
    builder.Services.AddAuthorization();

    builder.Services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        cfg.AddOpenBehavior(typeof(ReSys.Core.Common.Behaviors.ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    // 4. Seeding
    builder.Services.AddHostedService<ReSys.Identity.Seeding.IdentitySeeder>();

    var app = builder.Build();

    app.MapDefaultEndpoints();

    // Initialize DB (Apply Migrations)
    /*
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        db.Database.EnsureCreated();
    }
    */

    app.UseAuthentication();
    app.UseAuthorization(); // Not strictly needed for token endpoint but good practice

    app.MapCarter();

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