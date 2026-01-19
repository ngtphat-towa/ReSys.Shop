using Microsoft.Extensions.Options;

namespace ReSys.Infrastructure.Security.Authentication.Externals.Options;

public sealed class FacebookOption : IValidateOptions<FacebookOption>
{
    public const string Section = "Authentication:Facebook";

    public string AppId { get; init; } = string.Empty;
    public string AppSecret { get; init; } = string.Empty;

    public ValidateOptionsResult Validate(string? name, FacebookOption options)
    {
        if (string.IsNullOrWhiteSpace(options.AppId)) return ValidateOptionsResult.Fail("AppId is required.");
        return ValidateOptionsResult.Success;
    }
}