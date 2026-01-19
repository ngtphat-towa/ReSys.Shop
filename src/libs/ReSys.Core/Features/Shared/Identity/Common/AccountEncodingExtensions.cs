using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Common;

public static class AccountEncodingExtensions
{
    public static ErrorOr<string> DecodeToken(this string code)
    {
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch
        {
            return Error.Validation(code: "Auth.InvalidTokenFormat", description: "The security token is not in a valid format.");
        }
    }
}
