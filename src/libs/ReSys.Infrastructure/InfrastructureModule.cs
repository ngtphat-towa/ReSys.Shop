using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ReSys.Infrastructure;

public static class InfrastructureModule
{
    /// <summary>
    /// Adds all infrastructure modules. Typically used by the main API.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddPersistence(configuration)
            .AddStorage(configuration)
            .AddMlServices(configuration)
            .AddImaging(configuration)
            .AddNotifications(configuration);
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        Persistence.PersistenceModule.AddPersistence(services, configuration);
        return services;
    }

    /// <summary>
    /// Adds only the infrastructure modules needed by the Identity service.
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddNotifications(configuration);
    }

    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        Storage.StorageModule.AddStorage(services, configuration);
        return services;
    }

    public static IServiceCollection AddMlServices(this IServiceCollection services, IConfiguration configuration)
    {
        Ml.MlModule.AddMlServices(services, configuration);
        return services;
    }

    public static IServiceCollection AddImaging(this IServiceCollection services, IConfiguration configuration)
    {
        Imaging.ImagingModule.AddImaging(services, configuration);
        return services;
    }

    public static IServiceCollection AddNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        Notifications.NotificationsModule.AddNotifications(services, configuration);
        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        return app;
    }
}
