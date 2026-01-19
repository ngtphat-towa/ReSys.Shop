namespace ReSys.Core.Domain.Promotions.Calculations;

/// <summary>
/// Represents a single calculated discount to be applied to an order or line item.
/// </summary>
public record PromotionAdjustment(
    string Description,
    long AmountCents, // Minor units
    Guid? LineItemId = null);

/// <summary>
/// Immutable context containing all data required to evaluate a promotion against an order.
/// </summary>
public record PromotionCalculationContext(
    Promotion Promotion,
    ReSys.Core.Domain.Ordering.Order Order,
    IReadOnlyList<ReSys.Core.Domain.Ordering.LineItems.LineItem> EligibleItems);

/// <summary>
/// Final result of a promotion calculation.
/// </summary>
public record PromotionCalculationResult(
    Guid PromotionId,
    IReadOnlyList<PromotionAdjustment> Adjustments);