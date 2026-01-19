using MediatR;

using Microsoft.AspNetCore.Identity;

using FluentValidation;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Common;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Internal.Password;

public static class ResetPassword
{
    public record Request(string Email, string Code, string NewPassword);
    public record Command(Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Request.Code).NotEmpty();
            RuleFor(x => x.Request.NewPassword).NotEmpty().MinimumLength(8);
        }
    }

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null) return UserErrors.InvalidToken; // Security: Generic error

            var decodedTokenResult = request.Code.DecodeToken();
            if (decodedTokenResult.IsError) return decodedTokenResult.Errors;

            var result = await userManager.ResetPasswordAsync(user, decodedTokenResult.Value, request.NewPassword);

            if (!result.Succeeded)
                return result.Errors.ToApplicationResult(prefix: "ResetPassword");

            return Result.Success;
        }
    }
}