using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Ordering;

using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Adjustments;

/// <summary>
/// Represents a financial modification applied to the entire order (e.g., Shipping fee, Global discount).
/// It acts as a glue entity between the Order and the Pricing/Promotion engines.
/// Adjustments ensure that the audit trail for every cent added or removed from the total is preserved.
/// </summary>
public sealed class OrderAdjustment : Entity
{
    /// <summary>
    /// Defines the functional category of the adjustment.
    /// </summary>
    public enum AdjustmentScope 
    { 
        /// <summary>General discount or fee.</summary>
        Order, 
        /// <summary>Freight or logistics related surcharge.</summary>
        Shipping, 
        /// <summary>Governmental or regional tax.</summary>
        Tax 
    }

    #region Properties
    /// <summary>Parent order reference.</summary>
    public Guid OrderId { get; set; }

    /// <summary>The logical bucket this adjustment belongs to.</summary>
    public AdjustmentScope Scope { get; set; }

    /// <summary>
    /// If false, the adjustment is excluded from total calculations.
    /// Used for temporary or conditionally invalid promotions.
    /// </summary>
    public bool Eligible { get; set; } = true;

    /// <summary>
    /// If true, the record persists even if the amount is zero.
    /// Useful for showing "Free Shipping" as an explicit line item.
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>The source promotion that triggered this adjustment (if applicable).</summary>
    public Guid? PromotionId { get; set; }

    /// <summary>
    /// The financial value. 
    /// Negative indicates a credit/discount. Positive indicates a charge/fee.
    /// </summary>
    public long AmountCents { get; set; }

    /// <summary>Human-readable label shown on invoices (e.g. '10% Summer Discount').</summary>
    public string Description { get; set; } = string.Empty;

    // Relationships
    public Order Order { get; set; } = null!;

    /// <summary>Computed: Indicates if this adjustment originated from a marketing campaign.</summary>
    public bool IsPromotion => PromotionId.HasValue;
    #endregion

    public OrderAdjustment() { }

    /// <summary>
    /// Factory for creating an order-level financial adjustment.
    /// </summary>
    public static ErrorOr<OrderAdjustment> Create(
        Guid orderId,
        long amountCents,
        string description,
        AdjustmentScope scope,
        Guid? promotionId = null,
        bool eligible = true,
        bool mandatory = false)
    {
        // Guard: Traceability requires a description.
        if (string.IsNullOrWhiteSpace(description)) return OrderAdjustmentErrors.DescriptionRequired;

        return new OrderAdjustment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            AmountCents = amountCents,
            Description = description.Trim(),
            Scope = scope,
            PromotionId = promotionId,
            Eligible = eligible,
            Mandatory = mandatory,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Toggles the inclusion of this adjustment in the order totals.
    /// </summary>
    public void SetEligibility(bool eligible)
    {
        Eligible = eligible;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
