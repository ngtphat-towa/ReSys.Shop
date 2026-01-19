using Microsoft.AspNetCore.Authorization;
using ReSys.Core.Common.Security.Authorization.Claims;

namespace ReSys.Infrastructure.Security.Authorization;

/// <summary>
/// Custom authorization attribute that supports permissions, roles, and policies.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequestAuthorizeAttribute : AuthorizeAttribute
{
    public RequestAuthorizeAttribute(string? permissions = null, string? roles = null, string? policies = null)
    {
        Permissions = SplitAndClean(permissions);
        CustomRoles = SplitAndClean(roles);
        Policies = SplitAndClean(policies);

        Policy = BuildPolicy();
    }

    public string[]? Permissions { get; }
    public string[]? Policies { get; }
    public string[]? CustomRoles { get; }

    private static string[]? SplitAndClean(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
    }

    private string BuildPolicy()
    {
        List<string> parts = [];
        if (Permissions?.Length > 0) parts.Add($"{CustomClaim.Permission}:{string.Join(",", Permissions)}");
        if (Policies?.Length > 0) parts.Add($"{CustomClaim.Policy}:{string.Join(",", Policies)}");
        if (CustomRoles?.Length > 0) parts.Add($"{CustomClaim.Role}:{string.Join(",", CustomRoles)}");
        return string.Join(";", parts);
    }
}