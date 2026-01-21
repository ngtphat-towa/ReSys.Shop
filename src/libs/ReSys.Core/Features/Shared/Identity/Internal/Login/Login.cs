using MediatR;
using Microsoft.AspNetCore.Identity;
using Mapster;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Common;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Shared.Identity.Internal.Login;

public static class Login
{
    public record Request(string Credential, string Password, bool RememberMe = false, string? IpAddress = null);
    public record Response : AuthenticationResponse;
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Credential).NotEmpty();
            RuleFor(x => x.Request.Password).NotEmpty();
        }
    }

    public class Handler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var request = command.Request;

            var user = await FindUserAsync(request.Credential);
            if (user == null) return UserErrors.NotFound(request.Credential);

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded) return result.MapToError();

            user.RecordSignIn(request.IpAddress);
            await userManager.UpdateAsync(user);

            var accessTokenResult = await jwtTokenService.GenerateAccessTokenAsync(user, ct);
            if (accessTokenResult.IsError) return accessTokenResult.Errors;

            var refreshTokenResult = await refreshTokenService.GenerateRefreshTokenAsync(
                user.Id, request.IpAddress ?? "unknown", request.RememberMe, ct);

            if (refreshTokenResult.IsError) return refreshTokenResult.Errors;

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
