namespace ReSys.Core.Features.Identity.Admin.Permissions.Common;

/// <summary>
/// Represents a system permission for administrative listing.
/// </summary>
public record PermissionResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
}