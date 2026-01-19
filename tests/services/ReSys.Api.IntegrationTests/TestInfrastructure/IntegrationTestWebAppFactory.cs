using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Respawn;

using Testcontainers.PostgreSql;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Ml;
using ReSys.Infrastructure.Imaging.Options;
using ReSys.Infrastructure.Ml.Options;
using ReSys.Infrastructure.Notifications.Options;
using ReSys.Infrastructure.Storage.Options;
using ReSys.Infrastructure.Persistence;

using Npgsql;

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
                .WithImage("pgvector/pgvector:pg17")
                .WithDatabase("shopdb_test")
                .WithUsername("postgres")
                .WithPassword("password")
                .Build();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:shopdb"] = _connectionString
            });
        });

        builder.ConfigureServices(services =>
        {
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
                }).UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            var mlServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMlService));
            if (mlServiceDescriptor != null) services.Remove(mlServiceDescriptor);
            services.AddSingleton<IMlService, FakeMlService>();
        });
    }

    public async ValueTask InitializeAsync()
    {
        if (_dbContainer != null)
        {
            await _dbContainer.StartAsync();
            _connectionString = _dbContainer.GetConnectionString();
        }
        else
        {
            _connectionString = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
                ?? "Host=localhost;Database=shopdb_test;Username=postgres;Password=password";
        }

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Create respawner with a fresh connection
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public", "identity", "location", "catalog", "ordering", "system", "testing"],
            TablesToIgnore = ["spatial_ref_sys", "__EFMigrationsHistory"],
            WithReseed = true
        });
    }

    public async Task ResetDatabaseAsync()
    {
        // Reset with a fresh connection every time to avoid State conflicts
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public AppDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
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
