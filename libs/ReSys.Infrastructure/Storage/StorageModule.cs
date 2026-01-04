using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Storage;

namespace ReSys.Infrastructure.Storage;

public static class StorageModule
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        
        services.AddSingleton<IFileValidator, FileValidator>();
        services.AddSingleton<IFileSecurityService, FileSecurityService>();
        services.AddSingleton<IFileService, LocalFileService>();

        return services;
    }
}
