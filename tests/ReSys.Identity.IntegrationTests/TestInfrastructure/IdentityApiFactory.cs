using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using OpenIddict.Abstractions;
using ReSys.Core.Common.Constants;
using ReSys.Identity.Persistence;
using ReSys.Identity.Persistence.Constants;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Identity:Issuer"] = "https://localhost:5003",
                ["ConnectionStrings:identitydb"] = _dbContainer.GetConnectionString()
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // The DbContext is already registered via Aspire's AddNpgsqlDbContext in Program.cs.
            // Since we provided 'ConnectionStrings:identitydb' above, it will point to the Testcontainer.
            // We just need to ensure OpenIddict is correctly configured if not already.
            
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
            
            // Ensure schema exists
            await db.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS {Schemas.Identity};");
            await db.Database.EnsureCreatedAsync();
            
            // Seed for tests
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByClientIdAsync("test-client") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "test-client",
                    DisplayName = "Test Client",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + AuthConstants.Scopes.Roles,
                        Permissions.Prefixes.Scope + "permissions", // Added permission scope
                        Permissions.Prefixes.Scope + Scopes.OpenId,
                        Permissions.Prefixes.Scope + Scopes.Profile,
                        Permissions.Prefixes.Scope + "offline_access"
                    }
                });
            }

            if (await scopeManager.FindByNameAsync(Scopes.Roles) is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = Scopes.Roles,
                    Resources = { AuthConstants.Resources.ShopApi }
                });
            }

            if (await scopeManager.FindByNameAsync("permissions") is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "permissions",
                    Resources = { AuthConstants.Resources.ShopApi }
                });
            }

            if (await scopeManager.FindByNameAsync(Scopes.OpenId) is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = Scopes.OpenId,
                    Resources = { AuthConstants.Resources.ShopApi }
                });
            }
        }

        // Init Respawner with a dedicated connection
        _respawnerConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _respawnerConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_respawnerConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public", Schemas.Identity]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await InitializeRespawnerAsync();
        await _respawner.ResetAsync(_respawnerConnection);

        // Seed after reset
        using (var scope = Services.CreateScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByClientIdAsync("test-client") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "test-client",
                    DisplayName = "Test Client",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + AuthConstants.Scopes.Roles,
                        Permissions.Prefixes.Scope + "permissions",
                        Permissions.Prefixes.Scope + Scopes.OpenId,
                        Permissions.Prefixes.Scope + Scopes.Profile,
                        Permissions.Prefixes.Scope + "offline_access"
                    }
                });
            }
            
            // Ensure scopes exist (CreateAsync is idempotent if we check existence, but strictly speaking Respawner wipes tables, so we need to recreate)
             if (await scopeManager.FindByNameAsync(Scopes.Roles) is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = Scopes.Roles,
                    Resources = { AuthConstants.Resources.ShopApi }
                });
            }

            if (await scopeManager.FindByNameAsync("permissions") is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "permissions",
                    Resources = { AuthConstants.Resources.ShopApi }
                });
            }
        }
    }
}
