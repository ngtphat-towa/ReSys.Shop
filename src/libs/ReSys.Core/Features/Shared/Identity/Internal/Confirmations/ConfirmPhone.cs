using MediatR;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Internal.Confirmations;

public static class ConfirmPhone
{
    public record Request(string UserId, string PhoneNumber, string Code);
    public record Command(Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.UserId).NotEmpty();
            RuleFor(x => x.Request.PhoneNumber).NotEmpty();
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

            var result = await userManager.ChangePhoneNumberAsync(user, request.PhoneNumber, request.Code);
            
            if (!result.Succeeded)
                return result.Errors.ToApplicationResult(prefix: "ConfirmPhone");

            return Result.Success;
        }
    }
}
