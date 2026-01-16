using System.Security.Claims;

namespace ReSys.Shared.Extensions;

public static class AuthExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal? principal)
    {
        var id = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    public static string? GetUserName(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirstValue(ClaimTypes.Name);
    }

    public static string? GetEmail(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirstValue(ClaimTypes.Email);
    }

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal? principal)
    {
        return principal?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal? principal)
    {
        return principal?.FindAll("permission").Select(c => c.Value) ?? Enumerable.Empty<string>();
    }
}