using MediatR;

using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Features.Shared.Identity.Common;

using ErrorOr;

using System.Security.Claims;

namespace ReSys.Core.Features.Shared.Identity.Admin.Users.AssignPermissionToUser;

public static class AssignPermissionToUser
{
    public record Request(string PermissionName);
    public record Command(string UserId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch User
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user == null) return UserErrors.NotFound(command.UserId);

            // 2. Logic: Check if already has direct permission
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Any(c => c.Type == CustomClaim.Permission && c.Value == command.Request.PermissionName))
                return Result.Success;

            // 3. Persistence: Identity Store
            var result = await userManager.AddClaimAsync(user, new Claim(CustomClaim.Permission, command.Request.PermissionName));
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User.Permission");

            return Result.Success;
        }
    }
}
