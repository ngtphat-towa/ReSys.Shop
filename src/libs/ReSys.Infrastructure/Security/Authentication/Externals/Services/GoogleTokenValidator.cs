using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Security.Authentication.External;
using ReSys.Core.Common.Security.Authentication.External;
using ReSys.Infrastructure.Security.Authentication.Externals.Options;
using ErrorOr;

namespace ReSys.Infrastructure.Security.Authentication.Externals.Services;

public sealed class GoogleTokenValidator(
    IOptions<GoogleOption> googleSettings,
    ILogger<GoogleTokenValidator> logger) : IExternalTokenValidator
{
    private readonly GoogleOption _googleOptions = googleSettings.Value;

    public async Task<ErrorOr<ExternalUserTransfer>> ValidateTokenAsync(
        string provider,
        string? accessToken,
        string? idToken,
        string? authorizationCode,
        string? redirectUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_googleOptions.ClientId]
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new ExternalUserTransfer
                {
                    ProviderId = payload.Subject,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    ProfilePictureUrl = payload.Picture,
                    EmailVerified = payload.EmailVerified,
                    ProviderName = "google"
                };
            }

            return Error.Validation(code: "Token.Invalid", description: "No valid token provided for Google");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Google token");
            return Error.Failure(code: "Token.ValidationError", description: "Failed to validate Google token");
        }
    }
}
