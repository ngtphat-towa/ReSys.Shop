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
}
