using Microsoft.Extensions.Options;

namespace ReSys.Infrastructure.Security.Authentication.Externals.Options;

public sealed class GoogleOption : IValidateOptions<GoogleOption>
{
    public const string Section = "Authentication:Google";

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;

    public ValidateOptionsResult Validate(string? name, GoogleOption options)
    {
        if (string.IsNullOrWhiteSpace(options.ClientId)) return ValidateOptionsResult.Fail("ClientId is required.");
        return ValidateOptionsResult.Success;
    }
}
