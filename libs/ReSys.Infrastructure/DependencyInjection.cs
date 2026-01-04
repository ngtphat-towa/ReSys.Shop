using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure.AI;
using ReSys.Infrastructure.Imaging;
using ReSys.Infrastructure.Persistence;
using ReSys.Infrastructure.Storage;

namespace ReSys.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Infrastructure", "Core");

        services
            .AddPersistence(configuration)
            .AddStorage(configuration)
            .AddAI(configuration)
            .AddImaging(configuration);

        return services;
    }

    public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseInfrastructure(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        return app;
    }
}