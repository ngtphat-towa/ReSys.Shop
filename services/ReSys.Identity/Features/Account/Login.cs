using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Account;

public static class Login
{
    public record Request(string Email, string Password, bool RememberMe);

    public record Command(Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Data.Password).NotEmpty();
        }
    }

    public class Handler(UserManager<ApplicationUser> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Data.Email);
            if (user == null) return Error.Validation("Auth.InvalidCredentials", "Invalid login attempt");

            if (await userManager.IsLockedOutAsync(user))
            {
                return Error.Validation("Auth.LockedOut", "Account is locked out");
            }

            var result = await userManager.CheckPasswordAsync(user, request.Data.Password);

            if (result)
            {
                await userManager.ResetAccessFailedCountAsync(user);
                return Result.Success;
            }

            await userManager.AccessFailedAsync(user);
            return Error.Validation("Auth.InvalidCredentials", "Invalid login attempt");
        }
    }
}
