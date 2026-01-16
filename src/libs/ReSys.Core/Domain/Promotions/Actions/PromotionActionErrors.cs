using ErrorOr;

namespace ReSys.Core.Domain.Promotions.Actions;

public static class PromotionActionErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "PromotionAction.NotFound",
        description: $"Promotion action with ID '{id}' was not found.");

    public static Error Required => Error.Validation(
        code: "PromotionAction.Required",
        description: "Promotion action is required.");
}
