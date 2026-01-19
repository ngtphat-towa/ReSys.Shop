using MediatR;

using Microsoft.AspNetCore.Identity;

using FluentValidation;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Common;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Internal.Confirmations;

public static class ConfirmEmail
{
    public record Request(string UserId, string Code);
    public record Command(Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.UserId).NotEmpty();
            RuleFor(x => x.Request.Code).NotEmpty();
        }
    }

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return UserErrors.NotFound(request.UserId);

            if (user.EmailConfirmed) return Result.Success;

            var decodedTokenResult = request.Code.DecodeToken();
            if (decodedTokenResult.IsError) return decodedTokenResult.Errors;

            var result = await userManager.ConfirmEmailAsync(user, decodedTokenResult.Value);

            if (!result.Succeeded)
                return result.Errors.ToApplicationResult(prefix: "ConfirmEmail");

            return Result.Success;
        }
    }
}