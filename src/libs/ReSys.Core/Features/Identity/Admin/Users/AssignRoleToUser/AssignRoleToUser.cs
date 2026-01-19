using MediatR;

using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Features.Identity.Common;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Admin.Users.AssignRoleToUser;

public static class AssignRoleToUser
{
    public record Request(string RoleName);
    public record Command(string UserId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Handler(UserManager<User> userManager, RoleManager<Role> roleManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch User
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user == null) return UserErrors.NotFound(command.UserId);

            // 2. Validate Role Existence
            if (!await roleManager.RoleExistsAsync(command.Request.RoleName))
                return RoleErrors.NotFound;

            // 3. Domain Logic & Persistence: Identity Managers
            // Note: We use UserManager here because Identity handles the mapping table internally.
            var result = await userManager.AddToRoleAsync(user, command.Request.RoleName);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User.Role");

            return Result.Success;
        }
    }
}