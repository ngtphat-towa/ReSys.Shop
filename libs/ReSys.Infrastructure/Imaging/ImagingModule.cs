using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Imaging;

using ReSys.Core.Common.Telemetry;

namespace ReSys.Infrastructure.Imaging;

public static class ImagingModule
{
    public static IServiceCollection AddImaging(this IServiceCollection services)
    {
        services.RegisterModule("Infrastructure", "Imaging");
        services.AddSingleton<IImageService, ImageService>();
        return services;
    }
}
