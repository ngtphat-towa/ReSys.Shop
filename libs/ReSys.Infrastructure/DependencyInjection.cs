using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Interfaces;
using ReSys.Infrastructure.Data;
using ReSys.Infrastructure.Services;

namespace ReSys.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("shopdb");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
                npgsqlOptions.MigrationsAssembly("ReSys.Migrations");
            });
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        
        services.AddHttpClient("MlService", client =>
        {
            client.BaseAddress = new Uri("http://localhost:8000");
        });

        services.AddScoped<IMlService, MlService>();
        services.AddSingleton<IFileService, LocalFileService>();

        return services;
    }

    public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseInfrastructure(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // ensure database created
        context.Database.EnsureCreated();

        return app;
    }
}
