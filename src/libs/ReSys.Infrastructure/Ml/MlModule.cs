using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Ml;
using ReSys.Shared.Constants;
using ReSys.Shared.Telemetry;
using ReSys.Infrastructure.Ml.Options;
using ReSys.Infrastructure.Ml.Services;

namespace ReSys.Infrastructure.Ml;

public static class MlModule
{
    public static IServiceCollection AddMlServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule(ModuleNames.Infrastructure, "AI", s =>
        {
            var section = configuration.GetSection(MlOptions.SectionName);
            var options = section.Get<MlOptions>();

            s.AddOptions<MlOptions>()
                .Bind(section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            if (options == null || string.IsNullOrEmpty(options.ServiceUrl) || options.ServiceUrl == "http://fake-ml-service")
            {
                s.AddSingleton<IMlService, FakeMlService>();
            }
            else
            {
                s.AddHttpClient<IMlService, MlService>();
            }
        });

        return services;
    }
}
