using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Common.Security.Authorization.Claims;

namespace ReSys.Infrastructure.Security.Authorization.Requirements;

internal sealed class HasAuthorizeClaimRequirementHandler(
    IServiceProvider serviceProvider,
    ILogger<HasAuthorizeClaimRequirementHandler> logger) : AuthorizationHandler<HasAuthorizeClaimRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasAuthorizeClaimRequirement requirement)
    {
        using var scope = serviceProvider.CreateScope();
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        var authProvider = scope.ServiceProvider.GetRequiredService<IAuthorizeClaimDataProvider>();

        if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
        {
            context.Fail();
            return;
        }

        var authData = await authProvider.GetUserAuthorizationAsync(userContext.UserId);
        if (authData == null)
        {
            context.Fail();
            return;
        }

        // Validate Permissions
        foreach (var p in requirement.Permissions)
        {
            if (!authData.Permissions.Contains(p))
            {
                logger.LogWarning("User {UserId} missing permission {Permission}", userContext.UserId, p);
                context.Fail();
                return;
            }
        }

        // Validate Roles
        foreach (var r in requirement.Roles)
        {
            if (!authData.Roles.Contains(r))
            {
                logger.LogWarning("User {UserId} missing role {Role}", userContext.UserId, r);
                context.Fail();
                return;
            }
        }

        context.Succeed(requirement);
    }
}
