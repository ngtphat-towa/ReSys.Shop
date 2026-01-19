using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Promotions.Actions;
using ReSys.Core.Domain.Promotions.Rules;
using ErrorOr;

namespace ReSys.Core.Domain.Promotions;

/// <summary>
/// Root of the Promotions aggregate.
/// Orchestrates eligibility rules and discount actions for the storefront.
/// </summary>
public sealed class Promotion : Aggregate, IHasMetadata, ISoftDeletable
{
    public enum PromotionType { None, OrderDiscount, ItemDiscount, FreeShipping, BuyXGetY }
    public enum DiscountType { Percentage, FixedAmount }

    #region Properties
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Description { get; private set; }
    
    // Eligibility Thresholds
    public decimal? MinimumOrderAmount { get; private set; }
    public decimal? MaximumDiscountAmount { get; private set; }
    
    // Scheduling
    public DateTimeOffset? StartsAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    
    // Usage Control
    public int? UsageLimit { get; private set; }
    public int UsageCount { get; private set; }
    public bool RequiresCouponCode { get; private set; }
    
    // Status
    public bool Active { get; private set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // Relationships
    public PromotionAction? Action { get; private set; }
    public ICollection<PromotionRule> Rules { get; private set; } = new List<PromotionRule>();
    #endregion

    #region Computed
    public bool IsActive => Active && !IsDeleted &&
        (!StartsAt.HasValue || StartsAt <= DateTimeOffset.UtcNow) &&
        (!ExpiresAt.HasValue || ExpiresAt >= DateTimeOffset.UtcNow) &&
        (!UsageLimit.HasValue || UsageCount < UsageLimit);

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTimeOffset.UtcNow;
    #endregion

    private Promotion() { }

    /// <summary>
    /// Factory for creating a new promotional offer.
    /// </summary>
    public static ErrorOr<Promotion> Create(
        string name,
        string? code = null,
        string? description = null,
        decimal? minimumOrderAmount = null,
        decimal? maximumDiscountAmount = null,
        DateTimeOffset? startsAt = null,
        DateTimeOffset? expiresAt = null,
        int? usageLimit = null,
        bool requiresCouponCode = false)
    {
        // Guard: Required fields
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Promotion.NameRequired", "Promotion name is required.");
        
        // Guard: Constraints
        if (name.Length > PromotionConstraints.NameMaxLength) return PromotionErrors.DuplicateName(name); // Placeholder for generic too long if needed

        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code?.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            MinimumOrderAmount = minimumOrderAmount,
            MaximumDiscountAmount = maximumDiscountAmount,
            StartsAt = startsAt,
            ExpiresAt = expiresAt,
            UsageLimit = usageLimit,
            RequiresCouponCode = requiresCouponCode,
            Active = true
        };

        promotion.RaiseDomainEvent(new PromotionEvents.PromotionCreated(promotion.Id, promotion.Name));
        return promotion;
    }

    #region Domain Methods
    public ErrorOr<Success> UpdateDetails(
        string name, 
        string? description, 
        decimal? minAmount, 
        decimal? maxDiscount,
        DateTimeOffset? start,
        DateTimeOffset? end)
    {
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Promotion.NameRequired");

        Name = name.Trim();
        Description = description?.Trim();
        MinimumOrderAmount = minAmount;
        MaximumDiscountAmount = maxDiscount;
        StartsAt = start;
        ExpiresAt = end;

        RaiseDomainEvent(new PromotionEvents.PromotionUpdated(Id));
        return Result.Success;
    }

    public ErrorOr<Success> Activate()
    {
        if (IsExpired) return PromotionErrors.Expired;
        
        Active = true;
        RaiseDomainEvent(new PromotionEvents.PromotionActivated(Id));
        return Result.Success;
    }

    public void Deactivate()
    {
        Active = false;
        RaiseDomainEvent(new PromotionEvents.PromotionDeactivated(Id));
    }

    public ErrorOr<Success> RecordUsage(Guid orderId)
    {
        if (!IsActive) return PromotionErrors.NotActive;
        
        UsageCount++;
        RaiseDomainEvent(new PromotionEvents.PromotionUsed(Id, orderId));
        return Result.Success;
    }

    public void SetAction(PromotionAction action)
    {
        Action = action;
    }

    public void AddRule(PromotionRule rule)
    {
        Rules.Add(rule);
    }

    public void RemoveRule(Guid ruleId)
    {
        var rule = Rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null) Rules.Remove(rule);
    }
    #endregion
}