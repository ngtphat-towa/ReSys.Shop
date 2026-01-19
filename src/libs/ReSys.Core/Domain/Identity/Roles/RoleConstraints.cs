namespace ReSys.Core.Domain.Identity.Roles;

public static class RoleConstraints
{
    public const int NameMaxLength = 256;
    public const int DisplayNameMaxLength = 256;
    public const int DescriptionMaxLength = 1024;
    
    public const int MinPriority = -100;
    public const int MaxPriority = 100;
    public const int DefaultPriority = 0;
}
