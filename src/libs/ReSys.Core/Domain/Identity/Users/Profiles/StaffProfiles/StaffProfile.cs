using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles;

public sealed class StaffProfile : Entity, IHasMetadata
{
    public string UserId { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    private StaffProfile() { }

    internal static StaffProfile Create(string userId, string? employeeId = null)
    {
        return new StaffProfile
        {
            UserId = userId,
            EmployeeId = employeeId
        };
    }
}
