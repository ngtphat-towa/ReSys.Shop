using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.AI;

namespace ReSys.Infrastructure.AI;

public static class AiModule
{
    public static IServiceCollection AddAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MlOptions>(configuration.GetSection(MlOptions.SectionName));
        services.AddHttpClient<IMlService, MlService>();

        return services;
    }
}
