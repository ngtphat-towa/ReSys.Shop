using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Promotions.Rules;

/// <summary>
/// Defines a condition that must be met for a promotion to be applicable.
/// </summary>
public sealed class PromotionRule : Entity
{
    public enum RuleType
    {
        None,
        FirstOrder,
        ProductInclude,
        ProductExclude,
        CategoryInclude,
        CategoryExclude,
        MinimumQuantity,
        UserGroup
    }

    public Guid PromotionId { get; set; }
    public RuleType Type { get; set; }

    /// <summary>
    /// Strongly-typed parameters for the rule (e.g., quantity threshold, required IDs).
    /// Mapped as JSONB in PostgreSQL.
    /// </summary>
    public RuleParameters Parameters { get; set; } = new();

    private PromotionRule() { }

    public static ErrorOr<PromotionRule> Create(Guid promotionId, RuleType type, RuleParameters parameters)
    {
        if (type == RuleType.None) return Error.Validation("PromotionRule.TypeRequired");

        return new PromotionRule
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            Type = type,
            Parameters = parameters
        };
    }

    public void Update(RuleParameters parameters)
    {
        Parameters = parameters;
    }
}

/// <summary>
/// Base record for promotion rule configuration.
/// </summary>
public record RuleParameters
{
    public string? Value { get; init; }
    public List<Guid> TargetIds { get; init; } = [];
    public int? Threshold { get; init; }
}
