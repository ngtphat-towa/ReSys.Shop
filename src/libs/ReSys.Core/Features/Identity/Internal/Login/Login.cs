using MediatR;
using Microsoft.AspNetCore.Identity;
using Mapster;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Internal.Common;
using ReSys.Core.Features.Identity.Common;
using ErrorOr;
using FluentValidation;
using ReSys.Core.Common.Security.Authentication.Tokens.Models;

namespace ReSys.Core.Features.Identity.Internal.Login;

public static class Login
{
    // Request:
    public record Request(string Credential, string Password, bool RememberMe = false, string? IpAddress = null);

    // Response:
    public record Response : AuthenticationResponse;

    // Command:
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Credential).NotEmpty();
            RuleFor(x => x.Request.Password).NotEmpty();
        }
    }

    // Handler:
    public class Handler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // 1. Find User (Email or Username)
            var user = await FindUserAsync(request.Credential);
            if (user == null) return UserErrors.NotFound(request.Credential);

            // 2. Validate Credentials & Lockout
            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                // Addresses Gap #3: Granular Feedback
                return result.MapToError();
            }

            // 3. Domain Logic: Track Engagement
            user.RecordSignIn(request.IpAddress);
            await userManager.UpdateAsync(user);

            // 4. Token Generation
            var accessTokenResult = await jwtTokenService.GenerateAccessTokenAsync(user, cancellationToken);
            if (accessTokenResult.IsError) return accessTokenResult.Errors;

            var refreshTokenResult = await refreshTokenService.GenerateRefreshTokenAsync(
                user.Id,
                request.IpAddress ?? "unknown",
                request.RememberMe,
                cancellationToken);

            if (refreshTokenResult.IsError) return refreshTokenResult.Errors;

            // 5. Build Response
            var authResult = new AuthenticationResult
            {
                AccessToken = accessTokenResult.Value.Token,
                AccessTokenExpiresAt = accessTokenResult.Value.ExpiresAt,
                RefreshToken = refreshTokenResult.Value.Token,
                RefreshTokenExpiresAt = refreshTokenResult.Value.ExpiresAt
            };

            return authResult.Adapt<Response>();
        }

        private async Task<User?> FindUserAsync(string credential)
        {
            if (credential.Contains('@')) return await userManager.FindByEmailAsync(credential);
            return await userManager.FindByNameAsync(credential);
        }
    }
}