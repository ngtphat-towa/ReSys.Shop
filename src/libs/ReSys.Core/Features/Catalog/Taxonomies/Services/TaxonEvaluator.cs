using System.Linq.Expressions;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

namespace ReSys.Core.Features.Catalog.Taxonomies.Services;

public class TaxonEvaluator : ITaxonEvaluator
{
    public IQueryable<Product> ApplyTaxonRules(IQueryable<Product> query, Taxon taxon)
    {
        var rules = taxon.TaxonRules.ToList();
        if (!rules.Any()) return query.Where(x => false); // No rules, no matches for automatic taxons

        var parameter = Expression.Parameter(typeof(Product), "p");
        Expression? combinedExpression = null;

        foreach (var rule in rules)
        {
            var comparison = BuildRuleExpression(parameter, rule);
            if (comparison == null) continue;

            if (combinedExpression == null)
            {
                combinedExpression = comparison;
            }
            else
            {
                combinedExpression = taxon.RulesMatchPolicy.ToLower() == "any"
                    ? Expression.OrElse(combinedExpression, comparison)
                    : Expression.AndAlso(combinedExpression, comparison);
            }
        }

        if (combinedExpression == null) return query.Where(x => false);

        var lambda = Expression.Lambda<Func<Product, bool>>(combinedExpression, parameter);
        return query.Where(lambda);
    }

    private Expression? BuildRuleExpression(ParameterExpression parameter, TaxonRule rule)
    {
        return rule.Type switch
        {
            "product_name" => BuildStringExpression(Expression.Property(parameter, nameof(Product.Name)), rule),
            "product_description" => BuildStringExpression(Expression.Property(parameter, nameof(Product.Description)), rule),
            "product_status" => BuildEnumExpression<Product.ProductStatus>(Expression.Property(parameter, nameof(Product.Status)), rule),
            "product_price" => BuildNumericExpression(parameter, rule),
            _ => null
        };
    }

    private Expression BuildStringExpression(Expression property, TaxonRule rule)
    {
        var method = rule.MatchPolicy switch
        {
            "contains" => typeof(string).GetMethod("Contains", [typeof(string)]),
            "starts_with" => typeof(string).GetMethod("StartsWith", [typeof(string)]),
            "ends_with" => typeof(string).GetMethod("EndsWith", [typeof(string)]),
            _ => null
        };

        var value = Expression.Constant(rule.Value);

        if (method != null)
        {
            return Expression.Call(property, method, value);
        }

        return rule.MatchPolicy == "is_not_equal_to"
            ? Expression.NotEqual(property, value)
            : Expression.Equal(property, value);
    }

    private Expression BuildNumericExpression(ParameterExpression parameter, TaxonRule rule)
    {
        // Access p.Variants.FirstOrDefault(v => v.IsMaster).Price
        // For simple LINQ to SQL, we'll assume we can join or access Price directly if master variant is included.
        // Simplified for 4,000 records:
        var masterVariant = Expression.Call(typeof(Enumerable), "FirstOrDefault", [typeof(ReSys.Core.Domain.Catalog.Products.Variants.Variant)], 
            Expression.Property(parameter, nameof(Product.Variants)),
            Expression.Lambda<Func<ReSys.Core.Domain.Catalog.Products.Variants.Variant, bool>>(
                Expression.Property(Expression.Parameter(typeof(ReSys.Core.Domain.Catalog.Products.Variants.Variant), "v"), "IsMaster"),
                Expression.Parameter(typeof(ReSys.Core.Domain.Catalog.Products.Variants.Variant), "v")
            )
        );

        var priceProperty = Expression.Property(masterVariant, "Price");
        
        if (!decimal.TryParse(rule.Value, out var decimalValue)) return Expression.Constant(false);
        var constant = Expression.Constant(decimalValue);

        return rule.MatchPolicy switch
        {
            "greater_than" => Expression.GreaterThan(priceProperty, constant),
            "less_than" => Expression.LessThan(priceProperty, constant),
            "greater_than_or_equal" => Expression.GreaterThanOrEqual(priceProperty, constant),
            "less_than_or_equal" => Expression.LessThanOrEqual(priceProperty, constant),
            _ => Expression.Equal(priceProperty, constant)
        };
    }

    private Expression BuildEnumExpression<TEnum>(Expression property, TaxonRule rule) where TEnum : struct
    {
        if (!Enum.TryParse<TEnum>(rule.Value, true, out var enumValue)) return Expression.Constant(false);
        var constant = Expression.Constant(enumValue);

        return rule.MatchPolicy == "is_not_equal_to"
            ? Expression.NotEqual(property, constant)
            : Expression.Equal(property, constant);
    }
}
