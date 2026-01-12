namespace ReSys.Infrastructure.Authentication.Authorization;

public class PermissionAuthorizationOptions
{
    public const string SectionName = "Infrastructure:Authorization";

    public List<string> AuthenticationSchemes { get; set; } = [];
}
