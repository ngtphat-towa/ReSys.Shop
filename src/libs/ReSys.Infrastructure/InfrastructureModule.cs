using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Infrastructure.Security;

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
            .AddSecurity(configuration)
            .AddStorage(configuration)
            .AddMlServices(configuration)
            .AddImaging(configuration)
            .AddNotifications(configuration)
            .AddPayments(configuration)
            .AddJobs(configuration); // Background Jobs
    }

    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        Backgrounds.JobsModule.AddJobs(services, configuration);
        return services;
    }

    public static IServiceCollection AddPayments(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ReSys.Core.Domain.Ordering.Payments.Gateways.IPaymentProcessorFactory, Payments.PaymentFactory>();
        services.AddScoped<ReSys.Core.Domain.Ordering.Payments.Gateways.IPaymentProcessor, Payments.Gateways.StripeProcessor>();
        // Inventory
        services.AddScoped<ReSys.Core.Domain.Inventories.Services.IInventoryReservationService, Inventories.Services.InventoryReservationService>();
        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        Persistence.PersistenceModule.AddPersistence(services, configuration);
        return services;
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
