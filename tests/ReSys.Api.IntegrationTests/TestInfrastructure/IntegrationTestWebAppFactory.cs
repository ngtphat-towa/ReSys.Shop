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
