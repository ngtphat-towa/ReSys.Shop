using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Tokens;
using ReSys.Infrastructure.Security.Authentication.Tokens.Options;
using ErrorOr;

namespace ReSys.Infrastructure.Security.Authentication.Tokens.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> jwtSettings) : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions = jwtSettings.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public Task<ErrorOr<TokenResult>> GenerateAccessTokenAsync(User applicationUser, CancellationToken cancellationToken = default)
    {
        try
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset expires = now.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);

            List<Claim> claims = [
                new(JwtRegisteredClaimNames.Sub, applicationUser.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.UniqueName, applicationUser.UserName ?? string.Empty),
                new("email_verified", applicationUser.EmailConfirmed.ToString().ToLower())
            ];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires.UtcDateTime,
                signingCredentials: creds
            );

            return Task.FromResult<ErrorOr<TokenResult>>(new TokenResult(
                _tokenHandler.WriteToken(token),
                expires.ToUnixTimeSeconds()
            ));
        }
        catch
        {
            return Task.FromResult<ErrorOr<TokenResult>>(Jwt.Errors.GenerationFailed);
        }
    }

    public ErrorOr<ClaimsPrincipal> GetPrincipalFromToken(string token)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwtToken.Claims, "JWT");
            return new ClaimsPrincipal(identity);
        }
        catch { return Jwt.Errors.ParseFailed; }
    }

    public ErrorOr<bool> ValidateTokenFormat(string token)
    {
        return !string.IsNullOrWhiteSpace(token) && token.Split('.').Length == 3;
    }
}