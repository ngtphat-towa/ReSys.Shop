using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Storage;

using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure.Storage.Options;
using ReSys.Infrastructure.Storage.Services;
using ReSys.Infrastructure.Storage.Validators;

namespace ReSys.Infrastructure.Storage;

public static class StorageModule
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Infrastructure", "Storage", s =>
        {
            s.AddOptions<StorageOptions>()
                .Bind(configuration.GetSection(StorageOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            
            s.AddSingleton<IFileValidator, FileValidator>();
            s.AddSingleton<IFileSecurityService, FileSecurityService>();
            s.AddSingleton<IFileService, LocalFileService>();
        });

        return services;
    }
}
