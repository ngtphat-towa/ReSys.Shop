using System.Security.Claims;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Common;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Core.Features.Identity.Account.UpdateProfile;

public static class UpdateProfile
{
    public record Command(ClaimsPrincipal User, UpdateUserRequest Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Optional fields, but if provided should be valid if any rules exist
        }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(command.User);
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
