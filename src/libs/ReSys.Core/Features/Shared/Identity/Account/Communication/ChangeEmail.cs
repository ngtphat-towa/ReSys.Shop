using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Shared.Identity.Account.Communication;

public static class ChangeEmail
{
    public record Request(string NewEmail, string CurrentPassword);
    public record Response(string Message);
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.NewEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.Request.CurrentPassword).NotEmpty();
        }
    }

    public class Handler(
        IUserContext userContext,
        UserManager<User> userManager,
        INotificationService notificationService,
        IConfiguration configuration) : IRequestHandler<Command, ErrorOr<Response>>
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
            var existingUser = await userManager.FindByEmailAsync(req.NewEmail);
            if (existingUser != null && existingUser.Id != user.Id)
                return UserErrors.EmailAlreadyExists(req.NewEmail);

            // 3. Logic: Trigger Email Change Flow
            var result = await userManager.GenerateAndSendConfirmationEmailAsync(
                notificationService, configuration, user, req.NewEmail, ct);

            if (result.IsError) return result.Errors;

            return new Response($"A confirmation email has been sent to {req.NewEmail}. Please verify it to complete the change.");
        }
    }
}
