using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Ml;
using ReSys.Core.Common.Telemetry;
using ReSys.Infrastructure.Ml.Options;
using ReSys.Infrastructure.Ml.Services;

namespace ReSys.Infrastructure.Ml;

public static class MlModule
{
    public static IServiceCollection AddMlServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Infrastructure", "AI");

        services.AddOptions<MlOptions>()
            .Bind(configuration.GetSection(MlOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IMlService, MlService>();

        return services;
    }
}
