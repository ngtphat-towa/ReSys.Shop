using ErrorOr;

namespace ReSys.Core.Domain.Promotions.Rules;

public static class PromotionRuleErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "PromotionRule.NotFound",
        description: $"Promotion rule with ID '{id}' was not found.");

    public static Error ValueRequired => Error.Validation(
        code: "PromotionRule.ValueRequired",
        description: "Rule value is required.");
}
