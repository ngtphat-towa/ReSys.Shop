using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Promotions.Rules;

public sealed class PromotionRule : Aggregate
{
    public Guid PromotionId { get; private set; }
    public RuleType Type { get; private set; }
    public string Value { get; private set; } = string.Empty;

    private PromotionRule() { }

    public static ErrorOr<PromotionRule> Create(Guid promotionId, RuleType type, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return PromotionRuleErrors.ValueRequired;

        return new PromotionRule
        {
            PromotionId = promotionId,
            Type = type,
            Value = value.Trim()
        };
    }

    public enum RuleType
    {
        FirstOrder,
        ProductInclude,
        ProductExclude,
        CategoryInclude,
        CategoryExclude,
        MinimumQuantity,
        UserRole
    }
}
