using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

public static class TaxonRuleErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "TaxonRule.NotFound",
        description: $"Taxon rule with ID '{id}' was not found.");

    public static Error Duplicate => Error.Conflict(
        code: "TaxonRule.Duplicate",
        description: "A rule with the same type, value, and match policy already exists.");

    public static Error InvalidType => Error.Validation(
        code: "TaxonRule.InvalidType",
        description: $"Rule type must be one of: {string.Join(", ", TaxonRuleConstraints.RuleTypes)}");

    public static Error InvalidMatchPolicy => Error.Validation(
        code: "TaxonRule.InvalidMatchPolicy",
        description: $"Match policy must be one of: {string.Join(", ", TaxonRuleConstraints.MatchPolicies)}");

    public static Error PropertyNameRequired => Error.Validation(
        code: "TaxonRule.PropertyNameRequired",
        description: "Property name is required for 'product_property' rule type.");

    public static Error ValueRequired => Error.Validation(
        code: "TaxonRule.ValueRequired",
        description: "Rule value is required.");

    public static Error ValueTooLong => Error.Validation(
        code: "TaxonRule.ValueTooLong",
        description: $"Value cannot exceed {TaxonRuleConstraints.ValueMaxLength} characters.");
}
