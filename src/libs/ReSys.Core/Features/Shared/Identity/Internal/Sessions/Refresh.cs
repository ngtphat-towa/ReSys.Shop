using MediatR;

using Microsoft.AspNetCore.Identity;

using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Common;

using ErrorOr;

using FluentValidation;

using Mapster;
using ReSys.Core.Common.Security.Authentication.Tokens.Models;

namespace ReSys.Core.Features.Shared.Identity.Internal.Sessions;

public static class Refresh
{
    // Request:
    public record Request(string RefreshToken, bool RememberMe = false, string? IpAddress = null);

    // Response:
    public record Response : AuthenticationResponse;

    // Command:
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.RefreshToken).NotEmpty();
        }
    }

    // Handler:
    public class Handler(
        UserManager<User> userManager,
        IRefreshTokenService refreshTokenService,
        IJwtTokenService jwtTokenService) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var ipAddress = request.IpAddress ?? "unknown";

            // 1. Rotate the Refresh Token (Infrastructure Level)
            // This handles the family-based revocation and hashing internally
            var rotationResult = await refreshTokenService.RotateRefreshTokenAsync(
                request.RefreshToken,
                ipAddress,
                request.RememberMe,
                cancellationToken);

            if (rotationResult.IsError) return rotationResult.Errors;

            // 2. Load the User associated with the new token
            // We re-validate the user security status during refresh
            var principalResult = jwtTokenService.GetPrincipalFromToken(rotationResult.Value.Token);
            if (principalResult.IsError) return principalResult.Errors;

            var userId = principalResult.Value.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return UserErrors.InvalidToken;

            var user = await userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                // Security Rule: If user is inactive, revoke the new token immediately
                await refreshTokenService.RevokeTokenAsync(rotationResult.Value.Token, ipAddress, "Inactive User", cancellationToken);
                return UserErrors.AccountLocked;
            }

            // 3. Generate new Access Token
            var accessTokenResult = await jwtTokenService.GenerateAccessTokenAsync(user, cancellationToken);
            if (accessTokenResult.IsError) return accessTokenResult.Errors;

            // 4. Return combined result
            var authResult = new AuthenticationResult
            {
                AccessToken = accessTokenResult.Value.Token,
                AccessTokenExpiresAt = accessTokenResult.Value.ExpiresAt,
                RefreshToken = rotationResult.Value.Token,
                RefreshTokenExpiresAt = rotationResult.Value.ExpiresAt
            };

            return authResult.Adapt<Response>();
        }
    }
}
