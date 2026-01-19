using ReSys.Core.Domain.Promotions;

namespace ReSys.Core.Features.Promotions.Admin.Common;

/// <summary>
/// Core configuration parameters for a promotion.
/// </summary>
public record PromotionParameters
{
    public string Name { get; init; } = null!;
    public string? Code { get; init; }
    public string? Description { get; init; }
    
    public decimal? MinimumOrderAmount { get; init; }
    public decimal? MaximumDiscountAmount { get; init; }
    
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    
    public int? UsageLimit { get; init; }
    public bool RequiresCouponCode { get; init; }
}

/// <summary>
/// Detailed response for promotion listing and administration.
/// </summary>
public record PromotionResponse : PromotionParameters
{
    public Guid Id { get; init; }
    public int UsageCount { get; init; }
    public bool Active { get; init; }
    public bool IsActive { get; init; } // Computed status
    public bool IsExpired { get; init; }
}