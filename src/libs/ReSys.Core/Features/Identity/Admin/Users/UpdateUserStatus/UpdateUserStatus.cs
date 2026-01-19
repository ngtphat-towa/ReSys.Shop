using MediatR;

using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Common;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Admin.Users.UpdateUserStatus;

public static class UpdateUserStatus
{
    public record Request(bool IsActive);
    public record Command(string UserId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch User
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user == null) return UserErrors.NotFound(command.UserId);

            // 2. Domain Logic: Change Status
            var updateResult = user.UpdateStatus(command.Request.IsActive);
            if (updateResult.IsError) return updateResult.Errors;

            // 3. Persistence: Identity Store
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User");

            return Result.Success;
        }
    }
}