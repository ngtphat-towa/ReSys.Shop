namespace ReSys.Core.Features.Identity.Internal.Common;

/// <summary>
/// Common parameters for account registration or update.
/// </summary>
public record AccountParameters
{
    public string Email { get; init; } = null!;
    public string? UserName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

/// <summary>
/// Final response for a successful authentication attempt.
/// </summary>
public record AuthenticationResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public long AccessTokenExpiresAt { get; init; }
    public long RefreshTokenExpiresAt { get; init; }
    public string TokenType { get; init; } = "Bearer";
}

/// <summary>
/// User profile data intended for UI display.
/// </summary>
public record UserProfileResponse
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? UserName { get; init; }
    public string? FullName { get; init; }
    public string? ProfileImagePath { get; init; }
    public bool IsEmailConfirmed { get; init; }
    public DateTimeOffset? LastSignInAt { get; set; }
}

/// <summary>
/// Represents an active device/session for the user.
/// </summary>
public record ActiveSessionResponse
{
    public Guid Id { get; init; }
    public string CreatedByIp { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public bool IsCurrent { get; init; }
}