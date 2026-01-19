using MediatR;
using Microsoft.Extensions.Configuration;
using ReSys.Core.Features.Shared.Identity.External.Common;
using ReSys.Core.Common.Options.Systems;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.External.Config;

public static class GetExternalProviders
{
    public record Query : IRequest<ErrorOr<List<ExternalProviderModel>>>;

    public class Handler(IConfiguration configuration) : IRequestHandler<Query, ErrorOr<List<ExternalProviderModel>>>
    {
        public Task<ErrorOr<List<ExternalProviderModel>>> Handle(Query request, CancellationToken ct)
        {
            var providers = new List<ExternalProviderModel>();

            if (configuration.GetSection("Authentication:Google").Exists())
            {
                providers.Add(new ExternalProviderModel { 
                    Provider = "google", 
                    DisplayName = "Google", 
                    IsEnabled = true,
                    IconUrl = "https://developers.google.com/identity/images/g-logo.png"
                });
            }

            if (configuration.GetSection("Authentication:Facebook").Exists())
            {
                providers.Add(new ExternalProviderModel { 
                    Provider = "facebook", 
                    DisplayName = "Facebook", 
                    IsEnabled = true,
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/5/51/Facebook_f_logo_%282019%29.svg"
                });
            }

            return Task.FromResult<ErrorOr<List<ExternalProviderModel>>>(providers);
        }
    }
}
