using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Common.Security.Authentication.External;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.External.Common;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Shared.Identity.External.Login;

public static class ExchangeToken
{
    public record Request(
        string Provider,
        string? AccessToken = null,
        string? IdToken = null,
        string? AuthorizationCode = null,
        string? RedirectUri = null,
        bool RememberMe = false,
        string? IpAddress = null);

    public record Response : ExternalExchangeResponse;

    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Provider).NotEmpty();
            RuleFor(x => x.Request)
                .Must(r => !string.IsNullOrEmpty(r.AccessToken) || !string.IsNullOrEmpty(r.IdToken) || !string.IsNullOrEmpty(r.AuthorizationCode))
                .WithMessage("At least one token type (Access, Id, or AuthCode) must be provided.");
        }
    }

    public class Handler(
        UserManager<User> userManager,
        IExternalTokenValidator tokenValidator,
        IExternalUserService externalUserService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var req = command.Request;
            var ip = req.IpAddress ?? "unknown";

            // 1. Validate the External Token (Third-party SDK check)
            var validationResult = await tokenValidator.ValidateTokenAsync(
                req.Provider, req.AccessToken, req.IdToken, req.AuthorizationCode, req.RedirectUri, ct);

            if (validationResult.IsError) return validationResult.Errors;
            var externalInfo = validationResult.Value;

            // 2. Find, Link, or Provision User (Business Logic)
            var userResult = await externalUserService.FindOrCreateUserWithExternalLoginAsync(
                externalInfo, req.Provider, ct);

            if (userResult.IsError) return userResult.Errors;
            var (user, isNewUser, isNewLogin) = userResult.Value;

            // 3. Record Sign-In
            user.RecordSignIn(ip);
            await userManager.UpdateAsync(user);

            // 4. Generate Application Tokens
            var accessResult = await jwtTokenService.GenerateAccessTokenAsync(user, ct);
            if (accessResult.IsError) return accessResult.Errors;

            var refreshResult = await refreshTokenService.GenerateRefreshTokenAsync(
                user.Id, ip, req.RememberMe, ct);
            
            if (refreshResult.IsError) return refreshResult.Errors;

            return new Response
            {
                AccessToken = accessResult.Value.Token,
                RefreshToken = refreshResult.Value.Token,
                IsNewUser = isNewUser,
                IsNewLogin = isNewLogin
            };
        }
    }
}
