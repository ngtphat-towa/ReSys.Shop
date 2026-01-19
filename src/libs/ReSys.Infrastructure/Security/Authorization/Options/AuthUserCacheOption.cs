using Microsoft.Extensions.Options;

namespace ReSys.Infrastructure.Security.Authorization.Options;

public sealed class AuthUserCacheOption : IValidateOptions<AuthUserCacheOption>
{
    public const string Section = "Authorization:AuthUserCache";

    public int UserAuthCacheExpiryInMinutes { get; set; } = 60;
    public int UserAuthCacheSlidingInMinutes { get; set; } = 30;
    public int RoleClaimsCacheExpiryInMinutes { get; set; } = 120;

    public ValidateOptionsResult Validate(string? name, AuthUserCacheOption options)
    {
        if (options.UserAuthCacheExpiryInMinutes <= 0)
            return ValidateOptionsResult.Fail("UserAuthCacheExpiryInMinutes must be greater than 0.");

        return ValidateOptionsResult.Success;
    }
}