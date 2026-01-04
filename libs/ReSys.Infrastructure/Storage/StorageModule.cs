using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Storage;

using ReSys.Core.Common.Telemetry;

namespace ReSys.Infrastructure.Storage;

public static class StorageModule
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Infrastructure", "Storage");

        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddSingleton<IFileValidator, FileValidator>();
        services.AddSingleton<IFileSecurityService, FileSecurityService>();
        services.AddSingleton<IFileService, LocalFileService>();

        return services;
    }
}
