using Microsoft.Extensions.Options;
using OpenIddict.Validation;
using OpenIddict.Validation.SystemNetHttp;

namespace ReSys.Infrastructure.Identity;

public class ConfigureIdentityValidationOptions : IConfigureOptions<OpenIddictValidationOptions>
{
    private readonly IdentityValidationOptions _identityOptions;

    public ConfigureIdentityValidationOptions(IOptions<IdentityValidationOptions> identityOptions)
    {
        _identityOptions = identityOptions.Value;
    }

    public void Configure(OpenIddictValidationOptions options)
    {
        if (!string.IsNullOrEmpty(_identityOptions.Authority))
        {
            options.Issuer = new Uri(_identityOptions.Authority);
        }
    }
}
