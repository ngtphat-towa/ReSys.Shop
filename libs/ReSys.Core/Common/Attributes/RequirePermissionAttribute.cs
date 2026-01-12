using Microsoft.AspNetCore.Authorization;

namespace ReSys.Core.Common.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(params string[] permissions)
    {
        Permissions = permissions;
        Policy = $"{PolicyPrefix}{string.Join(",", permissions)}";
    }

    public string[] Permissions { get; }
}
