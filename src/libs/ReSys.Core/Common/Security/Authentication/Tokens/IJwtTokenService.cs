using System.Security.Claims;

using ReSys.Core.Domain.Identity.Users;

using ErrorOr;

namespace ReSys.Core.Common.Security.Authentication.Tokens;

public interface IJwtTokenService
{
    Task<ErrorOr<TokenResult>> GenerateAccessTokenAsync(User applicationUser, CancellationToken cancellationToken = default);
    ErrorOr<ClaimsPrincipal> GetPrincipalFromToken(string token);
    ErrorOr<bool> ValidateTokenFormat(string token);
}
