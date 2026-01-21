using ErrorOr;

namespace ReSys.Core.Domain.Promotions;

/// <summary>
/// Predefined business errors for the Promotions module.
/// </summary>
public static class PromotionErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Promotion.NotFound",
        description: $"Promotion with ID '{id}' was not found.");

    public static Error NotStarted => Error.Validation(
        code: "Promotion.NotStarted",
        description: "This promotion has not started yet.");

    public static Error Expired => Error.Validation(
        code: "Promotion.Expired",
        description: "This promotion has expired.");

    public static Error NotActive => Error.Validation(
        code: "Promotion.NotActive",
        description: "This promotion is currently inactive.");

    public static Error UsageLimitReached => Error.Validation(
        code: "Promotion.UsageLimitReached",
        description: "The usage limit for this promotion has been reached.");

    public static Error MinimumOrderNotMet(decimal minimum) => Error.Validation(
        code: "Promotion.MinimumOrderNotMet",
        description: $"Order total must be at least {minimum:C} to qualify for this promotion.");

    public static Error InvalidCode => Error.Validation(
        code: "Promotion.InvalidCode",
        description: "The provided promotion code is invalid.");

    public static Error DuplicateName(string name) => Error.Conflict(
        code: "Promotion.DuplicateName",
        description: $"A promotion with the name '{name}' already exists.");

    public static Error DuplicateCode(string code) => Error.Conflict(
        code: "Promotion.DuplicateCode",
        description: $"A promotion with the code '{code}' already exists.");

    public static Error ActionRequired => Error.Validation(
        code: "Promotion.ActionRequired",
        description: "A promotion must have a valid action configured.");
    
    public static Error InvalidRuleType => Error.Validation(
        code: "Promotion.InvalidRuleType",
        description: "Invalid promotion rule type.");
}
