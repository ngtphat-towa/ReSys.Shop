using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.UserGroups;
using ReSys.Core.Features.Shared.Identity.Common;
using ReSys.Core.Common.Data;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Shared.Identity.Admin.UserGroups.JoinGroup;

public static class JoinGroup
{
    public record Request(string UserId, bool IsPrimary = false);
    public record Command(Guid GroupId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.UserId).NotEmpty();
        }
    }

    public class Handler(
        UserManager<User> userManager,
        IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            // 1. Load Group
            var group = await context.Set<UserGroup>().AnyAsync(x => x.Id == command.GroupId, ct);
            if (!group) return UserGroupErrors.NotFound(command.GroupId);

            // 2. Load User with Memberships
            var user = await userManager.Users
                .Include(u => u.GroupMemberships)
                .FirstOrDefaultAsync(u => u.Id == command.Request.UserId, ct);

            if (user is null) return UserErrors.NotFound(command.Request.UserId);

            // 3. Domain Logic: Join Group
            var joinResult = user.JoinGroup(command.GroupId, command.Request.IsPrimary);
            if (joinResult.IsError) return joinResult.Errors;

            // 4. Persistence
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User.Group");

            return Result.Success;
        }
    }
}
