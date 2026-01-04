using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.AI;

using ReSys.Core.Common.Telemetry;

namespace ReSys.Infrastructure.AI;

public static class AiModule
{
    public static IServiceCollection AddAI(this IServiceCollection services, IConfiguration configuration)
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
