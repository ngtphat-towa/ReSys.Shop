using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles;

public static class StaffProfileErrors
{
    public static Error NotFound(string userId) => Error.NotFound(
        code: "StaffProfile.NotFound",
        description: $"Staff profile for user '{userId}' not found.");

    public static Error EmployeeIdRequired => Error.Validation(
        code: "StaffProfile.EmployeeIdRequired",
        description: "Employee ID is required for this operation.");

    public static Error JobTitleTooLong(int max) => Error.Validation(
        code: "StaffProfile.JobTitleTooLong",
        description: $"Job title cannot exceed {max} characters.");

    public static Error DepartmentTooLong(int max) => Error.Validation(
        code: "StaffProfile.DepartmentTooLong",
        description: $"Department cannot exceed {max} characters.");

    public static Error EmployeeIdTooLong(int max) => Error.Validation(
        code: "StaffProfile.EmployeeIdTooLong",
        description: $"Employee ID cannot exceed {max} characters.");
}
