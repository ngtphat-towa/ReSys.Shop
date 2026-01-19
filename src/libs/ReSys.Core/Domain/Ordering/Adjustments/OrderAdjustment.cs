using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Ordering;
using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Adjustments;

/// <summary>
/// Represents a financial modification applied to the entire order (e.g., Shipping fee, Global discount).
/// Glue entity between Order and Pricing engine.
/// </summary>
public sealed class OrderAdjustment : Entity
{
    public enum AdjustmentScope { Order, Shipping, Tax }

    public Guid OrderId { get; set; }
    public AdjustmentScope Scope { get; set; }
    public bool Eligible { get; set; } = true;
    
    /// <summary>If true, persists even if amount is zero (e.g., Free Shipping indicator).</summary>
    public bool Mandatory { get; set; }
    public Guid? PromotionId { get; set; }
    
    /// <summary>Negative for discount, positive for fee/tax.</summary>
    public long AmountCents { get; set; }
    public string Description { get; set; } = string.Empty;

    // Relationships
    public Order Order { get; set; } = null!;

    public bool IsPromotion => PromotionId.HasValue;

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

    public void SetEligibility(bool eligible)
    {
        Eligible = eligible;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}