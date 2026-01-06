namespace ReSys.Core.Features.Identity.Common;

public static class RoleConstraints
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 50;
}

public static class UserConstraints
{
    public const int PasswordMinLength = 6;
    public const int NameMaxLength = 100;
}
