using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReSys.Core.Interfaces;
using ReSys.Infrastructure.Data;
using ReSys.Infrastructure.Options;
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

        services.Configure<MlOptions>(configuration.GetSection(MlOptions.SectionName));
        
        services.AddHttpClient("MlService", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MlOptions>>().Value;
            client.BaseAddress = new Uri(options.ServiceUrl);
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
