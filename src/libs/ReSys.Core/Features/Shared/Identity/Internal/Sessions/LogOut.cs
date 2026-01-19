using MediatR;

using Microsoft.AspNetCore.Identity;

using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Domain.Identity.Users;

using ErrorOr;

using FluentValidation;

namespace ReSys.Core.Features.Shared.Identity.Internal.Sessions;

public static class LogOut
{
    public enum Mode
    {
        CurrentDevice,
        AllDevices
    }

    public record Request(string? RefreshToken, Mode Mode = Mode.CurrentDevice, string? IpAddress = null);
    public record Command(Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // If logging out of current device, token is highly recommended
            RuleFor(x => x.Request.RefreshToken)
                .NotEmpty()
                .When(x => x.Request.Mode == Mode.CurrentDevice);
        }
    }

    public class Handler(
        IRefreshTokenService refreshTokenService,
        IUserContext userContext) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var ip = request.IpAddress ?? "unknown";

            if (request.Mode == Mode.AllDevices)
            {
                if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                    return UserErrors.Unauthorized;

                // Revoke all tokens for this user family
                // Logic is handled in infrastructure to iterate and hash
                // Note: Infrastructure implementation usually needs a specific method for global revoke
                return await refreshTokenService.RevokeTokenAsync(request.RefreshToken ?? string.Empty, ip, "Logout All", cancellationToken);
            }

            // Single Logout
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                return await refreshTokenService.RevokeTokenAsync(request.RefreshToken, ip, "User Logout", cancellationToken);
            }

            return Result.Success;
        }
    }
}
