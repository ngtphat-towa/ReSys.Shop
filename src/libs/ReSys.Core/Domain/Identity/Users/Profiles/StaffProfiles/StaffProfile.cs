using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles;

/// <summary>
/// Represents additional profile information for system staff members.
/// Managed as a child entity of the User aggregate.
/// </summary>
public sealed class StaffProfile : Entity, IHasMetadata
{
    public string UserId { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public StaffProfile() { }

    /// <summary>
    /// Factory method for creating a staff profile.
    /// </summary>
    public static ErrorOr<StaffProfile> Create(string userId, string? employeeId = null)
    {
        // Guard: Validate EmployeeId length if provided
        if (employeeId?.Length > StaffProfileConstraints.EmployeeIdMaxLength)
            return StaffProfileErrors.EmployeeIdTooLong(StaffProfileConstraints.EmployeeIdMaxLength);

        return new StaffProfile
        {
            UserId = userId,
            EmployeeId = employeeId?.Trim()
        };
    }

    /// <summary>
    /// Updates staff-specific professional details.
    /// </summary>
    public ErrorOr<Success> Update(string? department, string? jobTitle, string? employeeId = null)
    {
        // Guard: Validate input lengths
        if (department?.Length > StaffProfileConstraints.DepartmentMaxLength)
            return StaffProfileErrors.DepartmentTooLong(StaffProfileConstraints.DepartmentMaxLength);

        if (jobTitle?.Length > StaffProfileConstraints.JobTitleMaxLength)
            return StaffProfileErrors.JobTitleTooLong(StaffProfileConstraints.JobTitleMaxLength);

        if (employeeId?.Length > StaffProfileConstraints.EmployeeIdMaxLength)
            return StaffProfileErrors.EmployeeIdTooLong(StaffProfileConstraints.EmployeeIdMaxLength);

        // Business Rule: Staff information is trimmed for consistency
        Department = department?.Trim();
        JobTitle = jobTitle?.Trim();
        if (employeeId != null) EmployeeId = employeeId.Trim();

        return Result.Success;
    }

    /// <summary>
    /// Sets metadata for the staff profile.
    /// </summary>
    public void SetMetadata(IDictionary<string, object?>? publicMetadata, IDictionary<string, object?>? privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
    }
}