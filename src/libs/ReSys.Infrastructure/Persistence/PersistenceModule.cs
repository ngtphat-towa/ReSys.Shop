using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Data;

using ReSys.Core.Common.Telemetry;

using ReSys.Infrastructure.Persistence.Interceptors;

namespace ReSys.Infrastructure.Persistence;

public static class PersistenceModule
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Infrastructure", "Persistence", s =>
        {
            var connectionString = configuration.GetConnectionString("shopdb");

            s.AddSingleton<AuditableEntityInterceptor>();

            s.AddDbContextPool<AppDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.UseVector();
                    npgsqlOptions.MigrationsAssembly("ReSys.Migrations");
                })
                .AddInterceptors(interceptor);
            });

            s.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        });

        return services;
    }
}
