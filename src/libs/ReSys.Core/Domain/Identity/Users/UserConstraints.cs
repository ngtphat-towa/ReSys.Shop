namespace ReSys.Core.Domain.Identity.Users;

public static class UserConstraints
{
    public const int FirstNameMaxLength = 100;
    public const int LastNameMaxLength = 100;
    public const int UserNameMaxLength = 256;
    public const int EmailMaxLength = 256;
    public const int PhoneMaxLength = 20;
    public const int ProfileImageMaxLength = 2048;
    public const int IpAddressMaxLength = 45;

    public const string UserNamePattern = @"^[a-zA-Z0-9_\-\.]+$";
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
}
