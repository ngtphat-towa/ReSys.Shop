using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Security.Authentication.External;
using ReSys.Core.Common.Security.Authentication.External;
using ErrorOr;

namespace ReSys.Infrastructure.Security.Authentication.Externals.Services;

public sealed class CompositeExternalTokenValidator(
    IServiceProvider serviceProvider,
    ILogger<CompositeExternalTokenValidator> logger)
    : IExternalTokenValidator
{
    public async Task<ErrorOr<ExternalUserTransfer>> ValidateTokenAsync(
        string provider,
        string? accessToken,
        string? idToken,
        string? authorizationCode,
        string? redirectUri,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Error.Validation(code: "Provider.Required", description: "Provider is required");
        }

        string normalizedProvider = provider.ToLowerInvariant();

        IExternalTokenValidator? validator = normalizedProvider switch
        {
            "google" => serviceProvider.GetService<GoogleTokenValidator>(),
            "facebook" => serviceProvider.GetService<FacebookTokenValidator>(),
            _ => null
        };

        if (validator == null)
        {
            logger.LogWarning("No validator found for provider: {Provider}", provider);
            return Error.NotFound(code: "Provider.ValidatorNotFound",
                description: $"No validator configured for provider '{provider}'. Supported providers: google, facebook");
        }

        return await validator.ValidateTokenAsync(provider, accessToken, idToken, authorizationCode, redirectUri, cancellationToken);
    }
}
