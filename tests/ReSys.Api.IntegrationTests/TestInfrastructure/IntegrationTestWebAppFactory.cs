using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Respawn;

using Testcontainers.PostgreSql;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.AI;
using ReSys.Infrastructure.ML;
using ReSys.Infrastructure.Persistence;
using ReSys.Infrastructure.Storage;
using ReSys.Infrastructure.Imaging;
using ReSys.Core.Common.Mailing;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace ReSys.Api.IntegrationTests.TestInfrastructure;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { 
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("permission", "Permissions.Users.Manage"), // Mock permission
            new Claim("permission", "Permissions.Roles.Manage")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer? _dbContainer;
    private string _connectionString = null!;
    private Respawner _respawner = null!;
    
    public TestMailService EmailSender { get; } = new();

    public JsonSerializerSettings JsonSettings { get; } = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
    };

    public System.Text.Json.JsonSerializerOptions DefaultJsonOptions { get; } = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public IntegrationTestWebAppFactory()
    {
        var useLocalDb = Environment.GetEnvironmentVariable("TEST_USE_LOCAL_DB") == "true";

        if (!useLocalDb)
        {
            _dbContainer = new PostgreSqlBuilder()
                .WithImage("ankane/pgvector")
                .Build();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("services:identity:http", "http://fake-identity-auth");
        // Aspire HealthChecks expect this
        builder.UseSetting("ConnectionStrings:shopdb", "Host=localhost;Database=dummy;Username=postgres;Password=password");
        // Prevent OTEL exporter errors if not configured
        builder.UseSetting("OTEL_EXPORTER_OTLP_ENDPOINT", "");

        builder.ConfigureServices(services =>
        {
            // Remove ALL registrations for AppDbContext (including pooling-related ones)
            var contextDescriptors = services.Where(d =>
                d.ServiceType == typeof(AppDbContext) ||
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(IApplicationDbContext) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericArguments().Contains(typeof(AppDbContext)))).ToList();

            foreach (var descriptor in contextDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.UseVector();
                });
            });

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            // Configure StorageOptions
            services.Configure<StorageOptions>(options =>
            {
                options.LocalPath = "test-storage";
                options.Security.EncryptionKey = "TestSecretKey123!";
            });

            // Configure ImageOptions
            services.Configure<ImageOptions>(options =>
            {
                // Increase limits for tests to accommodate sample assets
                options.MaxWidth = 8192;
                options.MaxHeight = 8192;
            });

            // Configure MlOptions to pass validation
            services.Configure<MlOptions>(options =>
            {
                options.ServiceUrl = "http://fake-ml-service";
            });

            // Replace IMlService with Fake
            var mlServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMlService));
            if (mlServiceDescriptor != null) services.Remove(mlServiceDescriptor);
            services.AddSingleton<IMlService, FakeMlService>();

            // Replace IMailService with TestMailService
            var mailServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMailService));
            if (mailServiceDescriptor != null) services.Remove(mailServiceDescriptor);
            services.AddSingleton<IMailService>(EmailSender);

            // Authentication bypass
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });
    }

    public async ValueTask InitializeAsync()
    {
        if (_dbContainer != null)
        {
            await _dbContainer.StartAsync();
            _connectionString = _dbContainer.GetConnectionString().Replace("Database=postgres", "Database=shopdb_test");
        }
        else
        {
            _connectionString = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
                ?? "Host=localhost;Database=shopdb_test;Username=postgres;Password=password";
        }

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_dbContainer != null)
        {
            await _dbContainer.StopAsync();
        }
        await base.DisposeAsync();
    }
}
