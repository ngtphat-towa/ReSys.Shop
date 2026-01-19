using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Ordering.LineItems;
using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Adjustments;

/// <summary>
/// Represents a financial modification applied specifically to one line item.
/// Glue entity between LineItem and Pricing engine.
/// </summary>
public sealed class LineItemAdjustment : Entity
{
    public Guid LineItemId { get; set; }
    public Guid? PromotionId { get; set; }
    
    /// <summary>Negative for discount, positive for fee/tax.</summary>
    public long AmountCents { get; set; }
    public string Description { get; set; } = string.Empty;
    
    /// <summary>If false, the adjustment is ignored in total calculations.</summary>
    public bool Eligible { get; set; } = true;

    // Relationships
    public LineItem LineItem { get; set; } = null!;

    public bool IsPromotion => PromotionId.HasValue;

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

    public void SetEligibility(bool eligible)
    {
        Eligible = eligible;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}