using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure.Imaging;
using ReSys.Infrastructure.Notifications;
using ReSys.Infrastructure.Persistence;
using ReSys.Infrastructure.Storage;
using ReSys.Infrastructure.Ml;

namespace ReSys.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Infrastructure", "Core", s =>
        {
            s.AddPersistence(configuration)
             .AddStorage(configuration)
             .AddMlServices(configuration)
             .AddImaging(configuration)
             .AddNotifications(configuration);
        });

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (env.IsDevelopment())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
            context.Database.Migrate();
        }

        return app;
    }
}