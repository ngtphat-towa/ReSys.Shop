using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Imaging;

namespace ReSys.Infrastructure.Imaging;

public static class ImagingModule
{
    public static IServiceCollection AddImaging(this IServiceCollection services)
    {
        Serilog.Log.Information("[Infrastructure] Initializing Imaging Module...");
        services.AddSingleton<IImageService, ImageService>();
        return services;
    }
}
