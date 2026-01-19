namespace ReSys.Core.Domain.Identity.UserGroups;

/// <summary>
/// Centralized constraints for the User Group aggregate.
/// </summary>
public static class UserGroupConstraints
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const int CodeMaxLength = 50;
}