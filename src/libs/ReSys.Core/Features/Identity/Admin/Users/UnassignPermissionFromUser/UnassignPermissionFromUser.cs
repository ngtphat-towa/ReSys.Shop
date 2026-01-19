using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Features.Identity.Common;
using ErrorOr;

namespace ReSys.Core.Features.Identity.Admin.Users.UnassignPermissionFromUser;

public static class UnassignPermissionFromUser
{
    public record Command(string UserId, string PermissionName) : IRequest<ErrorOr<Success>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch User
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user == null) return UserErrors.NotFound(command.UserId);

            // 2. Logic: Find specific direct claim
            var claims = await userManager.GetClaimsAsync(user);
            var claimToRemove = claims.FirstOrDefault(c => c.Type == CustomClaim.Permission && c.Value == command.PermissionName);

            if (claimToRemove == null) return Result.Success;

            // 3. Persistence
            var result = await userManager.RemoveClaimAsync(user, claimToRemove);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User.Permission");

            return Result.Success;
        }
    }
}