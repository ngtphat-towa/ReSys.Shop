using ReSys.Core.Common.Security.Authentication.Externals.Models;
using ErrorOr;

namespace ReSys.Core.Common.Security.Authentication.Externals.Interfaces;

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