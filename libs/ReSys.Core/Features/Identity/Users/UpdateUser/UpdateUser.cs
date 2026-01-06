using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Users.UpdateUser;

public static class UpdateUser
{
    public record Command(string Id, UpdateUserRequest Request) : IRequest<ErrorOr<Success>>;

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

            if (command.Request.FirstName != null) user.FirstName = command.Request.FirstName;
            if (command.Request.LastName != null) user.LastName = command.Request.LastName;
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }
            
            return Result.Success;
        }
    }
}
