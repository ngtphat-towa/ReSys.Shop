using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using ReSys.Core.Domain.Promotions.Rules;
using ReSys.Core.Domain.Promotions.Actions;

namespace ReSys.Core.Domain.Promotions.Promotions;

public sealed class Promotion : Aggregate, IHasMetadata
{
    public string Name { get; private set; } = string.Empty;
    public string? PromotionCode { get; private set; }
    public string? Description { get; private set; }
    public decimal? MinimumOrderAmount { get; private set; }
    public DateTimeOffset? StartsAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public int? UsageLimit { get; private set; }
    public int UsageCount { get; private set; }
    public bool Active { get; private set; } = true;

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; private set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; private set; } = new Dictionary<string, object?>();

    // Relationships
    public ICollection<PromotionRule> Rules { get; private set; } = new List<PromotionRule>();
    public ICollection<PromotionAction> Actions { get; private set; } = new List<PromotionAction>();

    private Promotion() { }

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
            Name = name.Trim(),
            PromotionCode = code?.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            UsageLimit = usageLimit
        };

        promotion.RaiseDomainEvent(new PromotionEvents.PromotionCreated(promotion));
        return promotion;
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

    public void IncrementUsage()
    {
        UsageCount++;
        RaiseDomainEvent(new PromotionEvents.PromotionUsed(this));
    }

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
}
