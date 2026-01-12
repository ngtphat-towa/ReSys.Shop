using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ReSys.Core.Common.Constants;
using ReSys.Core.Common.Interfaces;

namespace ReSys.Infrastructure.Authentication;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IImmutableList<string> Roles => User?.FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
        .ToImmutableList() ?? ImmutableList<string>.Empty;

    public IImmutableList<string> Permissions => User?.FindAll("permission")
        .Select(c => c.Value)
        .ToImmutableList() ?? ImmutableList<string>.Empty;

    public bool HasRole(string role) => Roles.Contains(role);

    public bool HasPermission(string permission) => Permissions.Contains(permission) || IsInAdminRole();

    public bool IsInAdminRole() => HasRole(AuthConstants.Roles.Admin);
}
