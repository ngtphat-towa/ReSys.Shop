using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;

public static class CustomerProfileErrors
{
    public static Error NotFound(string userId) => Error.NotFound(
        code: "CustomerProfile.NotFound",
        description: $"Customer profile for user '{userId}' not found.");

    public static Error InvalidCurrency => Error.Validation(
        code: "CustomerProfile.InvalidCurrency",
        description: "The provided currency code is invalid.");
}
