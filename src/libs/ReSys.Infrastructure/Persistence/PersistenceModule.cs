using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ReSys.Core.Common.Data;
using ReSys.Shared.Telemetry;
using ReSys.Infrastructure.Persistence.Interceptors;

using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence;

public static class PersistenceModule
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPostgresContext<AppDbContext, IApplicationDbContext>(configuration, ServiceNames.Database);
    }

    public static IServiceCollection AddPostgresContext<TDbContext, TContextInterface>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        string migrationsAssembly = "ReSys.Migrations")
        where TDbContext : DbContext, TContextInterface
        where TContextInterface : class
    {
        services.AddPostgresContext<TDbContext>(configuration, connectionStringName, migrationsAssembly);

        // Alias the interface to the implementation if not already registered
        services.TryAddScoped<TContextInterface>(sp => sp.GetRequiredService<TDbContext>());

        return services;
    }

    public static IServiceCollection AddPostgresContext<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        string migrationsAssembly = "ReSys.Migrations")
        where TDbContext : DbContext
    {
        services.RegisterModule(ModuleNames.Infrastructure, "Persistence", s =>
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);

            s.TryAddScoped<AuditableEntityInterceptor>();

            s.AddDbContext<TDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.UseVector();
                    npgsqlOptions.MigrationsAssembly(migrationsAssembly);
                })
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(interceptor);
            });
        });

        return services;
    }
}
