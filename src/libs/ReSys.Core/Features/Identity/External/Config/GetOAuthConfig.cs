using MediatR;
using Microsoft.Extensions.Configuration;
using ReSys.Core.Features.Identity.External.Common;
using ErrorOr;

namespace ReSys.Core.Features.Identity.External.Config;

public static class GetOAuthConfig
{
    public record Request(string Provider);
    public record Query(Request Request) : IRequest<ErrorOr<OAuthConfigResponse>>;

    public class Handler(IConfiguration configuration) : IRequestHandler<Query, ErrorOr<OAuthConfigResponse>>
    {
        public Task<ErrorOr<OAuthConfigResponse>> Handle(Query query, CancellationToken ct)
        {
            var provider = query.Request.Provider.ToLower();
            var section = configuration.GetSection($"Authentication:{provider}");

            if (!section.Exists()) return Task.FromResult<ErrorOr<OAuthConfigResponse>>(Error.NotFound("Provider.NotConfigured"));

            var response = provider switch
            {
                "google" => new OAuthConfigResponse
                {
                    Provider = "google",
                    ClientId = section["ClientId"] ?? string.Empty,
                    AuthorizationUrl = "https://accounts.google.com/o/oauth2/v2/auth",
                    TokenUrl = "https://oauth2.googleapis.com/token",
                    Scopes = ["openid", "email", "profile"]
                },
                "facebook" => new OAuthConfigResponse
                {
                    Provider = "facebook",
                    ClientId = section["AppId"] ?? string.Empty,
                    AuthorizationUrl = "https://www.facebook.com/v18.0/dialog/oauth",
                    TokenUrl = "https://graph.facebook.com/v18.0/oauth/access_token",
                    Scopes = ["email", "public_profile"]
                },
                _ => null
            };

            return response != null 
                ? Task.FromResult<ErrorOr<OAuthConfigResponse>>(response)
                : Task.FromResult<ErrorOr<OAuthConfigResponse>>(Error.Validation("Provider.NotSupported"));
        }
    }
}