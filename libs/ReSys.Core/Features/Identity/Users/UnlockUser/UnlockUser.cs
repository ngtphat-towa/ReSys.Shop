using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Users.UnlockUser;

public static class UnlockUser
{
    public record Command(string Id) : IRequest<ErrorOr<Success>>;

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

            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }
            
            return Result.Success;
        }
    }
}
