using Microsoft.AspNetCore.Http;

using ReSys.Shared.Extensions;

using System.Security.Claims;

namespace ReSys.Core.Common.Security.Authentication.Contexts;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal.GetUserId()?.ToString();

    public string? AdhocCustomerId { get; set; }

    public Guid? StoreId { get; set; }

    public string? UserName => Principal.GetUserName();

    public string? Email => Principal.GetEmail();

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles => Principal.GetRoles();

    public IEnumerable<string> Permissions => Principal.GetPermissions();

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public bool HasPermission(string permission) => 
        Permissions.Any(p => string.Equals(p, permission, StringComparison.OrdinalIgnoreCase));

    public void SetAdhocCustomerId(string adhocCustomerId)
    {
        AdhocCustomerId = adhocCustomerId;
    }

    public void SetStoreId(Guid storeId)
    {
        StoreId = storeId;
    }
}
