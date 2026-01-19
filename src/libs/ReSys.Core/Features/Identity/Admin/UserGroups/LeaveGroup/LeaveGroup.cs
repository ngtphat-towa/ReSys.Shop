using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Identity.Users;
using ErrorOr;

namespace ReSys.Core.Features.Identity.Admin.UserGroups.LeaveGroup;

public static class LeaveGroup
{
    public record Command(Guid GroupId, string UserId) : IRequest<ErrorOr<Success>>;

    public class Handler(
        UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            // 1. Load User with Memberships
            var user = await userManager.Users
                .Include(u => u.GroupMemberships)
                .FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

            if (user is null) return UserErrors.NotFound(command.UserId);

            // 2. Domain Logic: Leave Group
            var leaveResult = user.LeaveGroup(command.GroupId);
            if (leaveResult.IsError) return leaveResult.Errors;

            // 3. Persistence
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded ? Result.Success : Error.Failure("User.Group.LeaveFailed");
        }
    }
}