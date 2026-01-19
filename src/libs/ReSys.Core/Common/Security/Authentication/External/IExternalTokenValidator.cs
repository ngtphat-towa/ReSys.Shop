using ErrorOr;

namespace ReSys.Core.Common.Security.Authentication.External;

public interface IExternalTokenValidator
{
    Task<ErrorOr<ExternalUserTransfer>> ValidateTokenAsync(
        string provider,
        string? accessToken,
        string? idToken,
        string? authorizationCode,
        string? redirectUri,
        CancellationToken cancellationToken = default
    );
}
