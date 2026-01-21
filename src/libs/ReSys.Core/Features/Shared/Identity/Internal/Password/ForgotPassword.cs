using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Shared.Identity.Internal.Password;

public static class ForgotPassword
{
    public record Request(string Email);
    public record Response(string Message);
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        }
    }

    public class Handler(
        UserManager<User> userManager,
        IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var user = await userManager.FindByEmailAsync(command.Request.Email);
            
            if (user == null) 
                return new Response("If your email is registered, you will receive a reset link shortly.");

            user.RequestPasswordReset();
            await context.SaveChangesAsync(ct);

            return new Response("If your email is registered, you will receive a reset link shortly.");
        }
    }
}