using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Account;

public static class ConfirmEmailChange
{
    public record Request(string NewEmail, string Token);

    public record Command(string UserId, Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.NewEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.Data.Token).NotEmpty();
        }
    }

    public class Handler(UserManager<ApplicationUser> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return Error.NotFound("User.NotFound", "User not found");

            var result = await userManager.ChangeEmailAsync(user, request.Data.NewEmail, request.Data.Token);
            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            // Also update username if it's based on email
            await userManager.SetUserNameAsync(user, request.Data.NewEmail);

            return Result.Success;
        }
    }
}
