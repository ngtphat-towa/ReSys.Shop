using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

using Respawn;

using Testcontainers.PostgreSql;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using ReSys.Core.Common.Data;

using ReSys.Infrastructure.Persistence;
using ReSys.Core.Common.Ml;
using ReSys.Infrastructure.Imaging.Options;
using ReSys.Infrastructure.Ml.Options;
using ReSys.Infrastructure.Notifications.Options;
using ReSys.Infrastructure.Storage.Options;

using ReSys.Core.Common.Constants;
using ReSys.Infrastructure.Authentication.Authorization;

namespace ReSys.Api.IntegrationTests.TestInfrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer? _dbContainer;
    private string _connectionString = null!;
    private Respawner _respawner = null!;

    public JsonSerializerSettings JsonSettings { get; } = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        NullValueHandling = NullValueHandling.Ignore
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
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Only ConnectionStrings are kept here because PersistenceModule reads them directly from IConfiguration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:shopdb"] = _connectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // 1. Configure Options (Type-Safe)
            services.Configure<StorageOptions>(opts =>
            {
                opts.LocalPath = "test-storage";
                opts.Security.EncryptionKey = "TestSecretKey123!";
            });

            services.Configure<ImageOptions>(opts =>
            {
                opts.MaxWidth = 8192;
                opts.MaxHeight = 8192;
            });

            services.Configure<MlOptions>(opts =>
            {
                opts.ServiceUrl = "http://fake-ml-service";
            });

            services.Configure<SmtpOptions>(opts =>
            {
                opts.EnableEmailNotifications = false;
                opts.Provider = "logger";
            });

            services.Configure<SmsOptions>(opts =>
            {
                opts.EnableSmsNotifications = false;
                opts.Provider = "logger";
            });

            services.Configure<PermissionAuthorizationOptions>(opts =>
            {
                opts.AuthenticationSchemes.Add("TestScheme");
            });

            // 2. Database Modifications
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

            // 3. Service Replacements
            // Remove OpenIddict Validation to avoid 'no issuer' errors
            var openIddictDescriptors = services.Where(d => 
                d.ServiceType.Namespace?.StartsWith("OpenIddict") == true ||
                d.ImplementationType?.Namespace?.StartsWith("OpenIddict") == true).ToList();
            
            foreach (var descriptor in openIddictDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            services.AddAuthorization();

            // Replace IMlService with Fake
            var mlServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMlService));
            if (mlServiceDescriptor != null) services.Remove(mlServiceDescriptor);
            services.AddSingleton<IMlService, FakeMlService>();
        });
    }

    public class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = authHeader.ToString().Replace("Bearer ", "");

            if (token == "test-token")
            {
                var claims = new[] { new Claim(ClaimTypes.Name, "Admin User"), new Claim(ClaimTypes.Role, "Admin") };
                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "TestScheme");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            if (token == "test-token-customer")
            {
                var claims = new[] { new Claim(ClaimTypes.Name, "Customer User"), new Claim(ClaimTypes.Role, "Customer") };
                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "TestScheme");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            if (token == "test-token-with-perm")
            {
                var claims = new[] 
                { 
                    new Claim(ClaimTypes.Name, "Perm User"), 
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("permission", AppPermissions.Testing.ExampleCategories.Create) 
                };
                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "TestScheme");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }
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
