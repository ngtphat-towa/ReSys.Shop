using MediatR;

using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Common;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Admin.Users.UnassignRoleFromUser;

public static class UnassignRoleFromUser
{
    public record Command(string UserId, string RoleName) : IRequest<ErrorOr<Success>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch User
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user == null) return UserErrors.NotFound(command.UserId);

            // 2. Domain Logic & Persistence
            var result = await userManager.RemoveFromRoleAsync(user, command.RoleName);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User.Role");

            return Result.Success;
        }
    }
}