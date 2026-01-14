using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Imaging;
using ReSys.Shared.Constants;
using ReSys.Shared.Telemetry;
using ReSys.Infrastructure.Imaging.Options;
using ReSys.Infrastructure.Imaging.Services;

namespace ReSys.Infrastructure.Imaging;

public static class ImagingModule
{
    public static IServiceCollection AddImaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule(ModuleNames.Infrastructure, "Imaging", s =>
        {
            s.AddOptions<ImageOptions>()
                .Bind(configuration.GetSection(ImageOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            s.AddSingleton<IImageService, ImageService>();
        });
        return services;
    }
}
