namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

public static class TaxonRuleConstraints
{
    public const int TypeMaxLength = 50;
    public const int ValueMaxLength = 255;
    public const int PropertyNameMaxLength = 100;

    public static readonly string[] MatchPolicies =
    [
        "is_equal_to", "is_not_equal_to", "contains", "does_not_contain",
        "starts_with", "ends_with", "greater_than", "less_than",
        "greater_than_or_equal", "less_than_or_equal", "in", "not_in",
        "is_null", "is_not_null"
    ];

    public static readonly string[] RuleTypes =
    [
        "product_name", "product_sku", "product_description", "product_price",
        "product_weight", "product_available", "product_archived",
        "product_property", "variant_price", "variant_sku", "classification_taxon"
    ];
}