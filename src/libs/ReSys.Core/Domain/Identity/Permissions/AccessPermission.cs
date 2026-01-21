using System.Text.RegularExpressions;

using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Identity.Permissions;

/// <summary>
/// Represents a granular access permission in the system, defined by a hierarchical three-segment name:
/// <c>{area}.{resource}.{action}</c>.
/// </summary>
public sealed class AccessPermission : Aggregate, IHasMetadata
{
    #region Properties
    public string Name { get; set; } = null!;
    public string Area { get; set; } = null!;
    public string Resource { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Value { get; set; }
    public PermissionCategory Category { get; set; } = PermissionCategory.Both;

    // IHasMetadata implementation
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
    #endregion

    #region Enums
    public enum PermissionCategory
    {
        None = 0,
        User = 1,
        Role = 2,
        Both = 3
    }
    #endregion

    private AccessPermission() { }

    #region Factory Methods
    /// <summary>
    /// Creates a new AccessPermission with granular segments.
    /// </summary>
    public static ErrorOr<AccessPermission> Create(
        string area,
        string resource,
        string action,
        string? displayName = null,
        string? description = null,
        string? value = null,
        PermissionCategory category = PermissionCategory.Both)
    {
        // Guard: Required segments
        if (string.IsNullOrWhiteSpace(area)) return AccessPermissionErrors.AreaRequired;
        if (string.IsNullOrWhiteSpace(resource)) return AccessPermissionErrors.ResourceRequired;
        if (string.IsNullOrWhiteSpace(action)) return AccessPermissionErrors.ActionRequired;

        string trimmedArea = area.Trim().ToLowerInvariant();
        string trimmedResource = resource.Trim().ToLowerInvariant();
        string trimmedAction = action.Trim().ToLowerInvariant();

        // Guard: Segment patterns
        if (!IsValidSegment(trimmedArea) || !IsValidSegment(trimmedAction))
            return AccessPermissionErrors.InvalidFormat;

        if (!IsValidResourceSegment(trimmedResource))
            return AccessPermissionErrors.InvalidFormat;

        string name = $"{trimmedArea}.{trimmedResource}.{trimmedAction}";

        // Guard: Length constraints
        if (name.Length > AccessPermissionConstraints.MaxNameLength)
            return AccessPermissionErrors.NameTooLong;

        var permission = new AccessPermission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Area = trimmedArea,
            Resource = trimmedResource,
            Action = trimmedAction,
            DisplayName = displayName?.Trim() ?? GenerateDisplayName(trimmedArea, trimmedResource, trimmedAction),
            Description = description?.Trim() ?? GenerateDescription(trimmedArea, trimmedResource, trimmedAction),
            Value = value?.Trim(),
            Category = category,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        permission.RaiseDomainEvent(new AccessPermissionEvents.PermissionCreated(permission));
        return permission;
    }

    /// <summary>
    /// Creates a new AccessPermission from a full name string.
    /// </summary>
    public static ErrorOr<AccessPermission> Create(string name,
        string? displayName = null,
        string? description = null,
        string? value = null,
        PermissionCategory category = PermissionCategory.Both)
    {
        if (string.IsNullOrWhiteSpace(name))
            return AccessPermissionErrors.NameRequired;

        string[] parts = name.Trim().Split('.');

        if (parts.Length < AccessPermissionConstraints.MinSegments)
            return AccessPermissionErrors.InvalidFormat;

        string area = parts[0];
        string action = parts[^1];
        string resource = string.Join(".", parts.Skip(1).Take(parts.Length - 2));

        return Create(area, resource, action, displayName, description, value, category);
    }
    #endregion

    #region Domain Methods
    /// <summary>
    /// Updates permission display metadata and category.
    /// </summary>
    public ErrorOr<Success> Update(string? displayName, string? description, PermissionCategory? category = null)
    {
        DisplayName = displayName?.Trim() ?? DisplayName;
        Description = description?.Trim() ?? Description;
        if (category.HasValue) Category = category.Value;

        RaiseDomainEvent(new AccessPermissionEvents.PermissionUpdated(this));
        return Result.Success;
    }

    public void SetMetadata(IDictionary<string, object?>? publicMetadata, IDictionary<string, object?>? privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
    }
    #endregion

    #region Internal Helpers
    private static bool IsValidSegment(string segment) =>
        segment.Length >= AccessPermissionConstraints.MinSegmentLength &&
        segment.Length <= AccessPermissionConstraints.MaxSegmentLength &&
        Regex.IsMatch(segment, AccessPermissionConstraints.SegmentAllowedPattern);

    private static bool IsValidResourceSegment(string resource) =>
        resource.Split('.').All(IsValidSegment);

    private static string GenerateDisplayName(string area, string resource, string action) =>
        $"{Format(action)} {FormatResource(resource)}";

    private static string GenerateDescription(string area, string resource, string action) =>
        $"{Format(action)} {FormatResource(resource)} in {Format(area)} area";

    private static string Format(string s) => char.ToUpperInvariant(s[0]) + s[1..];
    private static string FormatResource(string r) => string.Join(" ", r.Split('.').Select(Format));
    #endregion
}
