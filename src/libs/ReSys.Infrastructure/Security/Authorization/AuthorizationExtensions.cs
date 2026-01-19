using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ReSys.Infrastructure.Security.Authorization;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Requires a single permission for the endpoint.
    /// </summary>
    public static TBuilder RequireAccessPermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(new RequestAuthorizeAttribute(permissions: permission));
    }

    /// <summary>
    /// Requires multiple permissions for the endpoint (AND logic).
    /// </summary>
    public static TBuilder RequireAccessPermissions<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(new RequestAuthorizeAttribute(permissions: string.Join(",", permissions)));
    }
}
