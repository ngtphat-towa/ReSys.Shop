using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Promotions.Usages;

/// <summary>
/// Represents a single instance of a promotion being successfully applied to an order.
/// Acts as an immutable audit record for financial and usage tracking.
/// </summary>
public sealed class PromotionUsage : Aggregate
{
    public Guid PromotionId { get; private set; }
    public Guid OrderId { get; private set; }
    public string? UserId { get; private set; }
    
    /// <summary>
    /// The total discount amount applied in this specific usage (in minor units/cents).
    /// </summary>
    public long DiscountAmountCents { get; private set; }
    
    /// <summary>
    /// Snapshot of the promotion code used (if any) at the time of application.
    /// </summary>
    public string? AppliedCode { get; private set; }

    public Promotion Promotion { get; private set; } = null!;

    private PromotionUsage() { }

    /// <summary>
    /// Factory for creating an immutable usage record.
    /// </summary>
    public static ErrorOr<PromotionUsage> Create(
        Guid promotionId, 
        Guid orderId, 
        long discountAmountCents, 
        string? userId = null, 
        string? appliedCode = null)
    {
        if (promotionId == Guid.Empty) return Error.Validation("PromotionUsage.PromotionIdRequired");
        if (orderId == Guid.Empty) return Error.Validation("PromotionUsage.OrderIdRequired");

        return new PromotionUsage
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            OrderId = orderId,
            UserId = userId,
            DiscountAmountCents = discountAmountCents,
            AppliedCode = appliedCode?.ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}