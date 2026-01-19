using System.Security.Cryptography;
using System.Text;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.Users;
using ErrorOr;

namespace ReSys.Core.Domain.Identity.Tokens;

/// <summary>
/// Represents a long-lived refresh token used to obtain new access tokens.
/// Implements token rotation and family-based revocation for high security.
/// </summary>
public sealed class RefreshToken : Aggregate
{
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? RevokedReason { get; set; }
    
    /// <summary>
    /// Identifies a chain of rotated tokens. Used to revoke all tokens in a family if reuse is detected.
    /// </summary>
    public string? TokenFamily { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    #region Factory Methods
    /// <summary>
    /// Creates a new refresh token for a user session.
    /// </summary>
    public static ErrorOr<RefreshToken> Create(
        User user,
        string token,
        TimeSpan lifetime,
        string ipAddress,
        string? tokenFamily = null)
    {
        try 
        {
            string rawToken = string.IsNullOrEmpty(token) ? GenerateRandomToken() : token;
            DateTimeOffset now = DateTimeOffset.UtcNow;

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = Hash(rawToken),
                CreatedAt = now,
                CreatedByIp = ipAddress.Trim(),
                ExpiresAt = now.Add(lifetime),
                TokenFamily = tokenFamily ?? Guid.NewGuid().ToString()
            };

            refreshToken.RaiseDomainEvent(new RefreshTokenEvents.TokenGenerated(refreshToken));
            return refreshToken;
        }
        catch 
        {
            return RefreshTokenErrors.GenerationFailed;
        }
    }
    #endregion

    #region Domain Methods
    /// <summary>
    /// Explicitly invalidates the token.
    /// </summary>
    public ErrorOr<Success> Revoke(string ipAddress, string? reason = null)
    {
        if (IsRevoked) return Result.Success;

        RevokedAt = DateTimeOffset.UtcNow;
        RevokedByIp = ipAddress.Trim();
        RevokedReason = reason?.Trim();

        RaiseDomainEvent(new RefreshTokenEvents.TokenRevoked(this, reason ?? "Manual Revocation"));
        return Result.Success;
    }

    /// <summary>
    /// Generates a cryptographically secure random token string.
    /// </summary>
    public static string GenerateRandomToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(RefreshTokenConstraints.TokenBytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Produces a one-way hash of the token for secure storage.
    /// </summary>
    public static string Hash(string rawToken)
    {
        using SHA512 sha = SHA512.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
    #endregion
}