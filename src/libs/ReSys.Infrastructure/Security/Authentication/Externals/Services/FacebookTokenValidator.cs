using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Security.Authentication.Externals.Interfaces;
using ReSys.Core.Common.Security.Authentication.Externals.Models;
using ReSys.Infrastructure.Security.Authentication.Externals.Options;
using ErrorOr;

namespace ReSys.Infrastructure.Security.Authentication.Externals.Services;

public sealed class FacebookTokenValidator(
    IOptions<FacebookOption> facebookSettings,
    ILogger<FacebookTokenValidator> logger,
    HttpClient httpClient) : IExternalTokenValidator
{
    private readonly FacebookOption _facebookOptions = facebookSettings.Value;

    public async Task<ErrorOr<ExternalUserTransfer>> ValidateTokenAsync(
        string provider,
        string? accessToken,
        string? idToken,
        string? authorizationCode,
        string? redirectUri,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return Error.Validation(code: "Token.Required", description: "Access token is required for Facebook");

        try
        {
            var response = await httpClient.GetAsync(
                $"https://graph.facebook.com/v18.0/me?fields=id,email,first_name,last_name,name,picture&access_token={accessToken}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Error.Unauthorized(code: "Facebook.Token.Invalid");

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<FacebookUserInfo>(content);

            if (userInfo == null) return Error.Failure("Facebook.UserInfo.Invalid");

            return new ExternalUserTransfer
            {
                ProviderId = userInfo.Id,
                Email = userInfo.Email ?? $"fb_{userInfo.Id}@facebook.local",
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                ProfilePictureUrl = userInfo.Picture?.Data?.Url,
                EmailVerified = !string.IsNullOrEmpty(userInfo.Email),
                ProviderName = "facebook"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Facebook token");
            return Error.Failure("Token.ValidationError");
        }
    }

    private sealed record FacebookUserInfo(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("first_name")] string? FirstName,
        [property: JsonPropertyName("last_name")] string? LastName,
        [property: JsonPropertyName("picture")] FacebookPicture? Picture);

    private sealed record FacebookPicture([property: JsonPropertyName("data")] FacebookPictureData? Data);
    private sealed record FacebookPictureData([property: JsonPropertyName("url")] string? Url);
}