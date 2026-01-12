using System.Security.Claims;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Identity.Authentication;

public static class OpenIddictHelpers
{
    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access token.
        // To include them, you must call SetDestinations() and specify Destinations.AccessToken.
        yield return Destinations.AccessToken;

        if (claim.Type is Claims.Name or Claims.Role or Claims.Email or "permission")
        {
            yield return Destinations.IdentityToken;
        }

        // Map roles to access token
        if (claim.Type is Claims.Role)
        {
            yield return Destinations.AccessToken;
        }

        // Note: "permission" claims are EXCLUDED from AccessToken here to prevent overload.
        // They will be re-injected via IClaimsTransformation on the receiving side.
    }
}
