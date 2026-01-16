using ErrorOr;

namespace ReSys.Core.Domain.Promotions.Promotions;

public static class PromotionErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Promotion.NotFound",
        description: $"Promotion with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "Promotion.NameRequired",
        description: "Name is required.");

    public static Error InvalidCode => Error.Validation(
        code: "Promotion.InvalidCode",
        description: "Invalid promotion code.");

    public static Error Expired => Error.Validation(
        code: "Promotion.Expired",
        description: "Promotion has expired.");

    public static Error UsageLimitReached => Error.Validation(
        code: "Promotion.UsageLimitReached",
        description: "Promotion usage limit has been reached.");
}
