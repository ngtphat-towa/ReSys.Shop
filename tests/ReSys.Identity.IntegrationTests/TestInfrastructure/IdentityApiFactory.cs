using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using ReSys.Identity.Persistence;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace ReSys.Identity.IntegrationTests.TestInfrastructure;

public class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _respawnerConnection = null!;

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        if (_respawnerConnection != null)
        {
            await _respawnerConnection.DisposeAsync();
        }
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<AppIdentityDbContext>));

            // Add DbContext pointing to Testcontainer
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.UseOpenIddict();
            });

            // Remove IdentitySeeder to avoid race conditions or failures in tests
            var seeder = services.FirstOrDefault(d => d.ImplementationType == typeof(ReSys.Identity.Seeding.IdentitySeeder));
            if (seeder != null)
            {
                services.Remove(seeder);
            }
        });
    }

    public async Task InitializeRespawnerAsync()
    {
        if (_respawner != null) return;

        // Ensure schema is created using a temporary scope
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        // Init Respawner with a dedicated connection
        _respawnerConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _respawnerConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_respawnerConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await InitializeRespawnerAsync();
        await _respawner.ResetAsync(_respawnerConnection);
    }
}