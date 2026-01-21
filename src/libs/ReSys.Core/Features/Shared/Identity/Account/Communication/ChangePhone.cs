using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;

using ErrorOr;

using FluentValidation;

namespace ReSys.Core.Features.Shared.Identity.Account.Communication;

public static class ChangePhone
{
    public record Request(string NewPhone, string CurrentPassword);
    public record Response(string Message);
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.NewPhone).NotEmpty().MaximumLength(UserConstraints.PhoneMaxLength);
            RuleFor(x => x.Request.CurrentPassword).NotEmpty();
        }
    }

    public class Handler(
        IUserContext userContext,
        UserManager<User> userManager,
        IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return UserErrors.Unauthorized;

            var user = await userManager.FindByIdAsync(userContext.UserId);
            if (user == null) return UserErrors.NotFound(userContext.UserId);

            var req = command.Request;

            // 1. Security Check
            if (!await userManager.CheckPasswordAsync(user, req.CurrentPassword))
                return UserErrors.InvalidCredentials;

            // 2. Uniqueness Check
            var phoneExists = await userManager.Users.AnyAsync(u => u.PhoneNumber == req.NewPhone && u.Id != user.Id, ct);
            if (phoneExists) return UserErrors.PhoneNumberAlreadyExists(req.NewPhone);

            // 3. Logic: Trigger SMS Change Flow via Domain Event
            user.RequestPhoneChange(req.NewPhone);
            context.Set<User>().Update(user);
            await context.SaveChangesAsync(ct);

            return new Response($"A confirmation code has been sent to {req.NewPhone}. Please verify it to complete the change.");
        }
    }
}
