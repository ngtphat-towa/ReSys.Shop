using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Users;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Admin.Users.DeleteAdminUser;

public static class DeleteAdminUser
{
    public record Command(string Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(command.Id);
            if (user == null) return UserErrors.NotFound(command.Id);

            // Business Rule: Check if user has active security constraints
            var deleteResult = user.Delete();
            if (deleteResult.IsError) return deleteResult.Errors;

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded) return Error.Failure("User.DeleteFailed");

            return Result.Deleted;
        }
    }
}
