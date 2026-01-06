using Carter;


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;


using OpenIddict.Server.AspNetCore;

namespace ReSys.Api.Features.Identity;

public class LogoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/connect/logout", async (HttpContext context) => await Logout(context));
        app.MapPost("/api/connect/logout", async (HttpContext context) => await Logout(context)).DisableAntiforgery();
    }

    private async Task<IResult> Logout(HttpContext httpContext)
    {
        // Ask ASP.NET Core Identity to delete the local and external cookies created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g Google or Facebook).
        await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application or to
        // the RedirectUri specified in the authentication properties if none was set.
        return Results.SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }
}
