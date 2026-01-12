using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;


using ReSys.Core.Common.Attributes;

namespace ReSys.Infrastructure.Authentication.Authorization;

public static class AuthorizationExtensions
{
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission) 
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization($"{RequirePermissionAttribute.PolicyPrefix}{permission}");
    }

    public static TBuilder RequirePermissions<TBuilder>(this TBuilder builder, params string[] permissions) 
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization($"{RequirePermissionAttribute.PolicyPrefix}{string.Join(",", permissions)}");
    }

    public static TBuilder RequireRoles<TBuilder>(this TBuilder builder, params string[] roles) 
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(new AuthorizeAttribute { Roles = string.Join(",", roles) });
    }

    public static TBuilder RequirePolicy<TBuilder>(this TBuilder builder, string policy) 
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(policy);
    }
}
