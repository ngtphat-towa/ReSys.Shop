using ErrorOr;

namespace ReSys.Core.Common.Security.Authentication.Tokens;

public interface IRefreshTokenService
{
    Task<ErrorOr<TokenResult>> GenerateRefreshTokenAsync(string userId, string ipAddress, bool rememberMe = false, CancellationToken cancellationToken = default);
    Task<ErrorOr<TokenResult>> RotateRefreshTokenAsync(string rawCurrentToken, string ipAddress, bool rememberMe = false, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> RevokeTokenAsync(string rawToken, string ipAddress, string? reason = null, CancellationToken cancellationToken = default);
}