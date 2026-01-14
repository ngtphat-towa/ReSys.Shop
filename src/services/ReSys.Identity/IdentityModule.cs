using System.Reflection;

using ReSys.Core;
using ReSys.Infrastructure;

namespace ReSys.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityProject(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreCommon(Assembly.GetExecutingAssembly());

        // Identity only needs specific infrastructure (e.g. Notifications)
        services.AddIdentityInfrastructure(configuration);

        return services;
    }

    public static IApplicationBuilder UseIdentityProject(this IApplicationBuilder app)
    {
        // Add middleware or configurations here if needed
        return app;
    }
}
