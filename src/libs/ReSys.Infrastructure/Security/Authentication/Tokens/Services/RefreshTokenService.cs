using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Domain.Identity.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Infrastructure.Security.Authentication.Tokens.Options;
using ErrorOr;

namespace ReSys.Infrastructure.Security.Authentication.Tokens.Services;

public sealed class RefreshTokenService(
    IApplicationDbContext dbContext,
    UserManager<User> userManager,
    IOptions<JwtOptions> options) : IRefreshTokenService
{
    private readonly JwtOptions _jwtOptions = options.Value;

    public async Task<ErrorOr<TokenResult>> GenerateRefreshTokenAsync(string userId, string ipAddress, bool rememberMe = false, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return UserErrors.NotFound(userId);

        int lifetimeDays = rememberMe ? _jwtOptions.RefreshTokenRememberMeLifetimeDays : _jwtOptions.RefreshTokenLifetimeDays;
        string rawToken = RefreshToken.GenerateRandomToken();
        
        var tokenResult = RefreshToken.Create(user, rawToken, TimeSpan.FromDays(lifetimeDays), ipAddress);
        if (tokenResult.IsError) return tokenResult.Errors;

        dbContext.Set<RefreshToken>().Add(tokenResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TokenResult(rawToken, tokenResult.Value.ExpiresAt.ToUnixTimeSeconds());
    }

    public async Task<ErrorOr<TokenResult>> RotateRefreshTokenAsync(string rawCurrentToken, string ipAddress, bool rememberMe = false, CancellationToken cancellationToken = default)
    {
        string hash = RefreshToken.Hash(rawCurrentToken);
        var oldToken = await dbContext.Set<RefreshToken>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (oldToken == null) return RefreshTokenErrors.NotFound;
        if (oldToken.IsRevoked) 
        {
            await RevokeTokenFamilyAsync(oldToken.TokenFamily!, ipAddress, "Token reuse detected", cancellationToken);
            return RefreshTokenErrors.Revoked;
        }
        if (oldToken.IsExpired) return RefreshTokenErrors.Expired;

        var user = oldToken.User;
        var newResult = await GenerateRefreshTokenAsync(user.Id, ipAddress, rememberMe, cancellationToken);
        
        if (!newResult.IsError)
        {
            oldToken.Revoke(ipAddress, "Token rotated");
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return newResult;
    }

    public async Task<ErrorOr<Success>> RevokeTokenAsync(string rawToken, string ipAddress, string? reason = null, CancellationToken cancellationToken = default)
    {
        string hash = RefreshToken.Hash(rawToken);
        var token = await dbContext.Set<RefreshToken>().FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        
        if (token != null)
        {
            token.Revoke(ipAddress, reason);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        
        return Result.Success;
    }

    private async Task RevokeTokenFamilyAsync(string family, string ipAddress, string reason, CancellationToken ct)
    {
        var tokens = await dbContext.Set<RefreshToken>()
            .Where(t => t.TokenFamily == family && !t.RevokedAt.HasValue)
            .ToListAsync(ct);

        foreach (var t in tokens) t.Revoke(ipAddress, reason);
        await dbContext.SaveChangesAsync(ct);
    }
}