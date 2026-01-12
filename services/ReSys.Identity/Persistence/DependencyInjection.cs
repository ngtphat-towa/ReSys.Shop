using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReSys.Core.Common.Telemetry;
using ReSys.Identity.Persistence;

namespace ReSys.Identity.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterModule("Infrastructure", "Persistence", s =>
        {
            builder.AddNpgsqlDbContext<AppIdentityDbContext>("identitydb", 
                settings => settings.DisableTracing = true, 
                options =>
                {
                    options.UseOpenIddict();
                });
        });

        return builder.Services;
    }
}
