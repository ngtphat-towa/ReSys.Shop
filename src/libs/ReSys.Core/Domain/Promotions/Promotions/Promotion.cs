using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using ReSys.Core.Domain.Promotions.Rules;
using ReSys.Core.Domain.Promotions.Actions;

namespace ReSys.Core.Domain.Promotions.Promotions;

/// <summary>
/// Represents a marketing campaign or discount rule within the system.
/// Orchestrates eligibility checks and discount applications for orders and items.
/// </summary>
public sealed class Promotion : Aggregate, IHasMetadata
{
    public enum PromotionType
    {
        None,
        OrderDiscount,
        ItemDiscount,
        FreeShipping,
        BuyXGetY
    }

    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }

    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? PromotionCode { get; set; }
    public string? Description { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool Active { get; set; } = true;

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // Relationships
    public ICollection<PromotionRule> Rules { get; set; } = new List<PromotionRule>();
    public ICollection<PromotionAction> Actions { get; set; } = new List<PromotionAction>();
    #endregion

    public Promotion() { }

    #region Factory Methods
    /// <summary>
    /// Factory for creating a new promotion campaign.
    /// </summary>
    public static ErrorOr<Promotion> Create(
        string name,
        string? code = null,
        string? description = null,
        int? usageLimit = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PromotionErrors.NameRequired;

        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            PromotionCode = code?.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            UsageLimit = usageLimit,
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        promotion.RaiseDomainEvent(new PromotionEvents.PromotionCreated(promotion));
        return promotion;
    }
    #endregion

    #region Business Logic
    /// <summary>
    /// Checks if the promotion is currently eligible for application.
    /// </summary>
    public ErrorOr<Success> CheckEligibility(DateTimeOffset now)
    {
        // Guard: Promotion must be active
        if (!Active) return PromotionErrors.NotFound(Id);

        // Guard: Check date range validity
        if (StartsAt.HasValue && now < StartsAt.Value) return PromotionErrors.InvalidCode;
        if (ExpiresAt.HasValue && now > ExpiresAt.Value) return PromotionErrors.Expired;

        // Guard: Check usage limits
        if (UsageLimit.HasValue && UsageCount >= UsageLimit.Value)
            return PromotionErrors.UsageLimitReached;

        return Result.Success;
    }

    /// <summary>
    /// Records a successful application of this promotion.
    /// </summary>
    public ErrorOr<Success> IncrementUsage()
    {
        // Business Rule: Re-verify eligibility before incrementing
        var eligibilityResult = CheckEligibility(DateTimeOffset.UtcNow);
        if (eligibilityResult.IsError) return eligibilityResult.Errors;

        UsageCount++;
        RaiseDomainEvent(new PromotionEvents.PromotionUsed(this));
        return Result.Success;
    }

    public ErrorOr<Success> Update(
        string? name = null,
        string? description = null,
        bool? active = null,
        IDictionary<string, object?>? publicMetadata = null,
        IDictionary<string, object?>? privateMetadata = null)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name)) return PromotionErrors.NameRequired;
            Name = name.Trim();
        }

        if (description != null) Description = description.Trim();
        if (active.HasValue) Active = active.Value;

        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);

        RaiseDomainEvent(new PromotionEvents.PromotionUpdated(this));
        return Result.Success;
    }
    #endregion
}
