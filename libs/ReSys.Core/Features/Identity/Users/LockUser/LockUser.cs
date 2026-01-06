using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Users.LockUser;

public static class LockUser
{
    public record Command(string Id, LockUserRequest Request) : IRequest<ErrorOr<Success>>;

    public class Handler : IRequestHandler<Command, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.Id);
            if (user == null) return IdentityErrors.UserNotFound;

            var lockoutEnd = command.Request.LockoutEnd ?? DateTimeOffset.MaxValue;
            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            
            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }
            
            return Result.Success;
        }
    }
}
