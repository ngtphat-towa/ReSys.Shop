using Microsoft.AspNetCore.Authorization;

namespace ReSys.Infrastructure.Authentication.Authorization;

public class PermissionRequirement(IEnumerable<string> permissions) : IAuthorizationRequirement
{
    public IEnumerable<string> Permissions { get; } = permissions;
}
