using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Promotions.Actions;

/// <summary>
/// Defines the behavior and parameters for a promotion's reward.
/// </summary>
public sealed class PromotionAction : Entity
{
    public Guid PromotionId { get; private set; }
    public Promotion.PromotionType Type { get; private set; }

    /// <summary>
    /// Strongly-typed parameters for the action (e.g., percentage, fixed amount, variant IDs).
    /// Mapped as JSONB in PostgreSQL.
    /// </summary>
    public ActionParameters Parameters { get; private set; } = new();

    private PromotionAction() { }

    public static ErrorOr<PromotionAction> Create(Guid promotionId, Promotion.PromotionType type, ActionParameters parameters)
    {
        if (type == Promotion.PromotionType.None) return PromotionErrors.ActionRequired;

        return new PromotionAction
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            Type = type,
            Parameters = parameters
        };
    }

    public void Update(ActionParameters parameters)
    {
        Parameters = parameters;
    }
}

/// <summary>
/// Base record for promotion action configuration.
/// </summary>
public record ActionParameters
{
    // Order/Item Discount
    public Promotion.DiscountType DiscountType { get; init; } = Promotion.DiscountType.Percentage;
    public decimal Value { get; init; }

    // Buy X Get Y
    public Guid? BuyVariantId { get; init; }
    public int? BuyQuantity { get; init; }
    public Guid? GetVariantId { get; init; }
    public int? GetQuantity { get; init; }
}