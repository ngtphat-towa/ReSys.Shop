using Microsoft.AspNetCore.Authorization;
using ReSys.Core.Common.Interfaces;

namespace ReSys.Infrastructure.Authentication.Authorization;

public class PermissionAuthorizationHandler(IUserContext userContext) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (requirement.Permissions.All(userContext.HasPermission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
