using ErrorOr;


using FluentValidation;


using MediatR;


using Microsoft.AspNetCore.Identity;


using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Account;

public static class ChangePassword
{
    public record Request(string CurrentPassword, string NewPassword, string ConfirmNewPassword);
    public record Command(string UserId, Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.CurrentPassword).NotEmpty();
            RuleFor(x => x.Data.NewPassword).NotEmpty().MinimumLength(6);
            RuleFor(x => x.Data.ConfirmNewPassword).Equal(x => x.Data.NewPassword);
        }
    }

    public class Handler(UserManager<ApplicationUser> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return Error.NotFound("Auth.UserNotFound", "User not found");

            var result = await userManager.ChangePasswordAsync(user, request.Data.CurrentPassword, request.Data.NewPassword);
            
            if (result.Succeeded) return Result.Success;

            return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
        }
    }
}
