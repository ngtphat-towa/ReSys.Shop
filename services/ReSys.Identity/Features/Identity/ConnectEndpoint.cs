using System.Security.Claims;
using Carter;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using ReSys.Core.Domain.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Identity.Features.Identity;

public class ConnectEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/connect/token", Exchange).DisableAntiforgery();
        app.MapGet("/connect/authorize", Authorize);
        app.MapPost("/connect/authorize", Authorize).DisableAntiforgery(); // Handle form post from login
    }

    private async Task<IResult> Exchange(
        HttpContext httpContext,
        IOpenIddictApplicationManager applicationManager,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var request = httpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return Results.BadRequest("The OpenID Connect request cannot be retrieved.");
        }

        if (request.IsClientCredentialsGrantType())
        {
            if (request.ClientId == null) return Results.BadRequest("Client ID is missing.");
            
            // Note: the client credentials are validated by OpenIddict automatically.
            var application = await applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                return Results.Problem("The application cannot be found.", statusCode: 400);
            }

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Use the client_id as the subject identifier.
            var clientId = await applicationManager.GetClientIdAsync(application) ?? "";
            identity.AddClaim(Claims.Subject, clientId);
            
            var clientName = await applicationManager.GetDisplayNameAsync(application) ?? "";
            identity.AddClaim(Claims.Name, clientName);
            
            // Set destinations separately
            foreach (var claim in identity.Claims)
            {
                claim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            }

            // Note: In a real world application, you'd probably want to add more claims.
            
            return Results.SignIn(new ClaimsPrincipal(identity), null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the user principal stored in the authorization code/refresh token.
            var principal = (await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            if (principal == null)
            {
                return Results.Challenge(properties: null, authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }
            
            // Retrieve the user to ensure they still exist and aren't locked out
            var user = await userManager.GetUserAsync(principal);
            if (user == null)
            {
                 return Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }
            
            if (!await signInManager.CanSignInAsync(user))
            {
                 return Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }
            
            // Create a new ClaimsPrincipal with the claims from the user
            var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            
            // Fix up scopes (ensure we keep the requested scopes)
             var scopes = request.GetScopes();
             newPrincipal.SetScopes(scopes);
             
             // Ensure destination of claims
             foreach (var claim in newPrincipal.Claims)
             {
                 claim.SetDestinations(GetDestinations(claim, newPrincipal));
             }

            return Results.SignIn(newPrincipal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Results.BadRequest(new { error = "unsupported_grant_type" });
    }

    private async Task<IResult> Authorize(
        HttpContext httpContext,
        IOpenIddictApplicationManager applicationManager,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var request = httpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return Results.BadRequest("The OpenID Connect request cannot be retrieved.");
        }
        
        // 1. Retrieve the user principal stored in the authentication cookie.
        // This checks if the user is logged into the ASP.NET Core Identity cookie
        var result = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        
        // 2. If not logged in, challenge the Identity cookie scheme (redirects to /login)
        if (!result.Succeeded)
        {
            return Results.Challenge(properties: null, authenticationSchemes: [IdentityConstants.ApplicationScheme]);
        }
        
        // 3. User is logged in. Create the principal for OpenIddict.
        var user = await userManager.GetUserAsync(result.Principal);
        if (user == null)
        {
            return Results.Problem("User not found.");
        }

        // Create the principal using the factory to ensure all claims are present
        var principal = await signInManager.CreateUserPrincipalAsync(user);

        // Set the scopes requested by the client
        principal.SetScopes(request.GetScopes());

        // Set destinations (put claims in Access Token vs Identity Token)
        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        // Return the SignIn result which OpenIddict intercepts to generate the Code
        return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        // Always include in Access Token
        yield return Destinations.AccessToken;

        // Include in Identity Token if it's a specific OpenID claim or if the user requested the 'profile'/'email' scope
        if (claim.Type == Claims.Name || claim.Type == Claims.Email)
        {
            if (principal.HasScope(Scopes.Profile) || principal.HasScope(Scopes.Email))
                yield return Destinations.IdentityToken;
        }
    }
}
