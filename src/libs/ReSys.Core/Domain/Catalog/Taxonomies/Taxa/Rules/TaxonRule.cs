using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

public sealed class TaxonRule : Entity
{
    public Guid TaxonId { get; private set; }
    public string Type { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public string MatchPolicy { get; private set; } = null!;
    public string? PropertyName { get; private set; }

    // Navigation
    public Taxon Taxon { get; private set; } = null!;

    private TaxonRule() { }

    public static ErrorOr<TaxonRule> Create(
        Guid taxonId,
        string type,
        string value,
        string? matchPolicy = null,
        string? propertyName = null)
    {
        if (string.IsNullOrWhiteSpace(value)) return TaxonRuleErrors.ValueRequired;
        if (value.Length > TaxonRuleConstraints.ValueMaxLength) return TaxonRuleErrors.ValueTooLong;

        var normalizedType = type.Trim().ToLowerInvariant();
        if (!TaxonRuleConstraints.RuleTypes.Contains(normalizedType))
            return TaxonRuleErrors.InvalidType;

        var policy = matchPolicy?.Trim().ToLowerInvariant() ?? TaxonRuleConstraints.MatchPolicies[0];
        if (!TaxonRuleConstraints.MatchPolicies.Contains(policy))
            return TaxonRuleErrors.InvalidMatchPolicy;

        if (normalizedType == "product_property" && string.IsNullOrWhiteSpace(propertyName))
            return TaxonRuleErrors.PropertyNameRequired;

        return new TaxonRule
        {
            TaxonId = taxonId,
            Type = normalizedType,
            Value = value.Trim(),
            MatchPolicy = policy,
            PropertyName = propertyName?.Trim()
        };
    }

    public ErrorOr<Success> Update(
        string? type = null,
        string? value = null,
        string? matchPolicy = null,
        string? propertyName = null)
    {
        if (value is not null)
        {
            if (string.IsNullOrWhiteSpace(value)) return TaxonRuleErrors.ValueRequired;
            if (value.Length > TaxonRuleConstraints.ValueMaxLength) return TaxonRuleErrors.ValueTooLong;
            Value = value.Trim();
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalized = type.Trim().ToLowerInvariant();
            if (!TaxonRuleConstraints.RuleTypes.Contains(normalized))
                return TaxonRuleErrors.InvalidType;
            Type = normalized;
        }

        if (!string.IsNullOrWhiteSpace(matchPolicy))
        {
            var policy = matchPolicy.Trim().ToLowerInvariant();
            if (!TaxonRuleConstraints.MatchPolicies.Contains(policy))
                return TaxonRuleErrors.InvalidMatchPolicy;
            MatchPolicy = policy;
        }

        if (propertyName is not null)
        {
            var trimmed = propertyName.Trim();
            if (Type == "product_property" && string.IsNullOrWhiteSpace(trimmed))
                return TaxonRuleErrors.PropertyNameRequired;
            PropertyName = trimmed;
        }

        return Result.Success;
    }

    public bool CanConvertToQueryFilter()
    {
        return Type switch
        {
            "product_name" => true,
            "product_description" => true,
            "product_status" => true,
            _ => false
        };
    }

    public string GetFieldName()
    {
        return Type switch
        {
            "product_name" => "Name",
            "product_description" => "Description",
            "product_status" => "Status",
            "is_digital" => "IsDigital",
            _ => string.Empty
        };
    }

    public string GetFilterOperator()
    {
        return MatchPolicy switch
        {
            "is_equal_to" => "=",
            "is_not_equal_to" => "!=",
            "greater_than" => ">",
            "less_than" => "<",
            "greater_than_or_equal" => ">=",
            "less_than_or_equal" => "<=",
            "contains" => "*",
            "starts_with" => "^",
            "ends_with" => "$",
            _ => "="
        };
    }
}