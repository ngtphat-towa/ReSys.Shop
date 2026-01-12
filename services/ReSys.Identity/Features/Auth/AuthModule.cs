using Carter;


using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;


using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;


using ReSys.Identity.Authentication;
using ReSys.Identity.Domain;


using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Identity.Features.Auth;

public class AuthModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/connect/token", async (
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var request = httpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                return Results.BadRequest("Invalid request");
            }

            if (request.IsPasswordGrantType())
            {
                var user = await userManager.FindByNameAsync(request.Username!);
                if (user == null)
                {
                    return Results.Problem("Invalid username or password", statusCode: 400);
                }

                var result = await signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                     return Results.Problem("Invalid username or password", statusCode: 400);
                }

                // Create the principal
                var principal = await signInManager.CreateUserPrincipalAsync(user);

                // Set the list of scopes granted to the client application.
                principal.SetScopes(request.GetScopes());

                // Ensure the subject claim is present and valid
                var subject = await userManager.GetUserIdAsync(user);
                principal.SetClaim(Claims.Subject, subject);

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(OpenIddictHelpers.GetDestinations(claim));
                }

                return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            
            if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                var result = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                var principal = result.Principal;
                
                if (principal == null)
                {
                     return Results.Problem("Invalid refresh token", statusCode: 400);
                }

                // Ensure the user is still allowed to sign in.
                var user = await userManager.GetUserAsync(principal);
                if (user == null || !await signInManager.CanSignInAsync(user))
                {
                    return Results.Problem("The user is no longer allowed to sign in.", statusCode: 400);
                }

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(OpenIddictHelpers.GetDestinations(claim));
                }

                return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return Results.Problem("The specified grant type is not implemented.", statusCode: 400);
        });
    }
}
