using Microsoft.AspNetCore.Http;
using ReSys.Shared.Models.Auth;
using ReSys.Shared.Extensions;
using System.Security.Claims;

namespace ReSys.Core.Common.Services.Auth;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => Principal.GetUserId();

    public string? UserName => Principal.GetUserName();

    public string? Email => Principal.GetEmail();

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles => Principal.GetRoles();

    public IEnumerable<string> Permissions => Principal.GetPermissions();

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public bool HasPermission(string permission) => 
        Permissions.Any(p => string.Equals(p, permission, StringComparison.OrdinalIgnoreCase));
}