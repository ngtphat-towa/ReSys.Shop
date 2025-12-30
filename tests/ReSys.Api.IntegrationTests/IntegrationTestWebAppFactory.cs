using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace ReSys.Api.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer? _dbContainer;
    private string _connectionString = null!;

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
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_connectionString, npgsqlOptions => 
                {
                    npgsqlOptions.UseVector();
                });
            });
        });
    }

    public async Task InitializeAsync()
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
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_dbContainer != null)
        {
            await _dbContainer.StopAsync();
        }
    }
}
