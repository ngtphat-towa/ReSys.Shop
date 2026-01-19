using System.Text;
using Microsoft.Extensions.Options;

namespace ReSys.Infrastructure.Security.Authentication.Tokens.Options;

public sealed class JwtOptions : IValidateOptions<JwtOptions>
{
    public const string Section = "Authentication:Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 15;
    public int RefreshTokenLifetimeDays { get; init; } = 7;
    public int RefreshTokenRememberMeLifetimeDays { get; init; } = 30;

    public int MaxActiveRefreshTokensPerUser { get; init; } = 5;
    public int RevokedTokenRetentionDays { get; init; } = 5;

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.Issuer)) failures.Add("JwtOptions.Issuer is required.");
        if (string.IsNullOrWhiteSpace(options.Audience)) failures.Add("JwtOptions.Audience is required.");
        if (string.IsNullOrWhiteSpace(options.Secret)) failures.Add("JwtOptions.Secret is required.");

        if (!string.IsNullOrWhiteSpace(options.Secret))
        {
            if (Encoding.UTF8.GetBytes(options.Secret).Length < 32) 
                failures.Add("JwtOptions.Secret must be at least 32 characters (256 bits).");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
