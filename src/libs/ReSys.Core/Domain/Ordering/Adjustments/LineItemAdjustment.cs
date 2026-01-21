using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Ordering.LineItems;

using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Adjustments;

/// <summary>
/// Represents a financial modification applied specifically to one line item.
/// It acts as a bridge between a specific product instance and the discount/tax rules.
/// This allows for granular reporting on which specific items were discounted.
/// </summary>
public sealed class LineItemAdjustment : Entity
{
    #region Properties
    /// <summary>Target item reference.</summary>
    public Guid LineItemId { get; set; }

    /// <summary>The source promotion that triggered this discount (if any).</summary>
    public Guid? PromotionId { get; set; }

    /// <summary>
    /// The financial value. 
    /// Negative for item-level discounts. Positive for item-level fees.
    /// </summary>
    public long AmountCents { get; set; }

    /// <summary>Human-readable reason for the adjustment.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// If false, the adjustment is ignored during the calculation of LineItem.TotalCents.
    /// </summary>
    public bool Eligible { get; set; } = true;

    // Relationships
    public LineItem LineItem { get; set; } = null!;

    /// <summary>Computed: Indicates if this adjustment is marketing-related.</summary>
    public bool IsPromotion => PromotionId.HasValue;
    #endregion

    public LineItemAdjustment() { }

    /// <summary>
    /// Factory for creating an item-level financial adjustment.
    /// </summary>
    public static ErrorOr<LineItemAdjustment> Create(
        Guid lineItemId,
        long amountCents,
        string description,
        Guid? promotionId = null,
        bool eligible = true)
    {
        // Guard: ERP data quality requires descriptive reasoning for all money movements.
        if (string.IsNullOrWhiteSpace(description)) return LineItemAdjustmentErrors.DescriptionRequired;

        return new LineItemAdjustment
        {
            Id = Guid.NewGuid(),
            LineItemId = lineItemId,
            AmountCents = amountCents,
            Description = description.Trim(),
            PromotionId = promotionId,
            Eligible = eligible,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Manually toggles the eligibility status. 
    /// Triggers a total recalculation in the parent aggregate.
    /// </summary>
    public void SetEligibility(bool eligible)
    {
        Eligible = eligible;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
