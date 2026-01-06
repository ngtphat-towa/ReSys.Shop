using Carter;


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;


using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;


using ReSys.Core.Domain.Identity;


using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Api.Features.Identity;

public class UserInfoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/connect/userinfo", UserInfo)
            .RequireAuthorization(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
        app.MapPost("/api/connect/userinfo", UserInfo)
            .RequireAuthorization(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)
            .DisableAntiforgery();
    }

    private async Task<IResult> UserInfo(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user == null)
        {
            return Results.Challenge(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            [Claims.Subject] = await userManager.GetUserIdAsync(user)
        };

        if (httpContext.User.HasScope(Scopes.Profile))
        {
            claims[Claims.Name] = await userManager.GetUserNameAsync(user) ?? "";
            claims[Claims.GivenName] = user.FirstName ?? "";
            claims[Claims.FamilyName] = user.LastName ?? "";
            claims["user_type"] = user.UserType.ToString();
        }

        if (httpContext.User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = await userManager.GetEmailAsync(user) ?? "";
            claims[Claims.EmailVerified] = await userManager.IsEmailConfirmedAsync(user);
        }

        if (httpContext.User.HasScope(Scopes.Roles))
        {
            claims[Claims.Role] = await userManager.GetRolesAsync(user);
        }

        // Note: the complete list of standard claims supported by the OpenID Connect specification
        // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

        return Results.Json(claims);
    }
}
