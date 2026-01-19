namespace ReSys.Core.Features.Shared.Identity.Admin.Users.Common;

/// <summary>
/// Summary data for user listing in the admin panel.
/// </summary>
public record AdminUserListItem
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? UserName { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Full detail view of a user for administrative auditing.
/// </summary>
public record AdminUserDetailResponse : AdminUserListItem
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public int AccessFailedCount { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public DateTimeOffset? LastSignInAt { get; init; }
    public string? LastIpAddress { get; init; }
}
