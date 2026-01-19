namespace ReSys.Core.Features.Shared.Identity.Admin.Roles.Common;

/// <summary>
/// Core parameters for creating or updating a role.
/// </summary>
public record RoleParameters
{
    public string Name { get; init; } = null!;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public int Priority { get; init; }
}

/// <summary>
/// Detailed response for a role aggregate.
/// </summary>
public record RoleResponse : RoleParameters
{
    public string Id { get; init; } = null!;
    public bool IsSystemRole { get; init; }
    public bool IsDefault { get; init; }
    public int UserCount { get; init; }
}
