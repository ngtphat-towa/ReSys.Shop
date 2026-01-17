using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Common.Extensions.Filters;
using ErrorOr;

namespace ReSys.Core.Features.Catalog.Taxonomies.Services;

/// <inheritdoc cref="ITaxonRegenerationService"/>
public sealed class TaxonRegenerationService(
    IApplicationDbContext context,
    ILogger<TaxonRegenerationService> logger) : ITaxonRegenerationService
{
    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> RegenerateProductsForTaxonAsync(Guid taxonId, CancellationToken ct)
    {
        logger.LogInformation("Starting product classification regeneration for Taxon {TaxonId}", taxonId);
        
        try
        {
            var taxon = await context.Set<Taxon>()
                .Include(t => t.TaxonRules)
                .Include(t => t.Classifications)
                .FirstOrDefaultAsync(t => t.Id == taxonId, ct);

            if (taxon == null)
            {
                logger.LogWarning("Taxon {TaxonId} not found", taxonId);
                return TaxonErrors.NotFound(taxonId);
            }

            if (!taxon.Automatic)
            {
                logger.LogInformation("Taxon {TaxonId} is manual. Skipping automatic regeneration", taxonId);
                return Result.Success;
            }

            // 1. Find matching products
            var matchingProductIds = await FindMatchingProductsAsync(taxon, ct);

            // 2. Sync classifications
            await SyncClassificationsAsync(taxon, matchingProductIds, ct);

            logger.LogInformation("Finished product classification regeneration for Taxon {TaxonId}", taxonId);
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during product classification regeneration for Taxon {TaxonId}", taxonId);
            return TaxonErrors.RegenerationFailed;
        }
    }

    private async Task<HashSet<Guid>> FindMatchingProductsAsync(Taxon taxon, CancellationToken ct)
    {
        var baseQuery = context.Set<Product>()
            .Where(p => p.Status == Product.ProductStatus.Active && !p.IsDeleted);

        if (!taxon.TaxonRules.Any())
        {
            return new HashSet<Guid>();
        }

        var rules = taxon.TaxonRules.ToList();
        
        if (taxon.RulesMatchPolicy == "all")
        {
            return await BuildAllMatchPolicyQuery(baseQuery, rules, ct);
        }
        else // "any"
        {
            return await BuildAnyMatchPolicyQuery(baseQuery, rules, ct);
        }
    }

    private async Task<HashSet<Guid>> BuildAllMatchPolicyQuery(IQueryable<Product> query, List<TaxonRule> rules, CancellationToken ct)
    {
        foreach (var rule in rules)
        {
            if (rule.CanConvertToQueryFilter())
            {
                var filterStr = $"{rule.GetFieldName()}{rule.GetFilterOperator()}{rule.Value}";
                query = query.ApplyDynamicFilter(filterStr);
            }
            else
            {
                query = ApplyCollectionRule(query, rule);
            }
        }

        return (await query.Select(p => p.Id).ToListAsync(ct)).ToHashSet();
    }

    private async Task<HashSet<Guid>> BuildAnyMatchPolicyQuery(IQueryable<Product> baseQuery, List<TaxonRule> rules, CancellationToken ct)
    {
        var allMatchingIds = new HashSet<Guid>();

        foreach (var rule in rules)
        {
            var query = baseQuery;
            if (rule.CanConvertToQueryFilter())
            {
                var filterStr = $"{rule.GetFieldName()}{rule.GetFilterOperator()}{rule.Value}";
                query = query.ApplyDynamicFilter(filterStr);
            }
            else
            {
                query = ApplyCollectionRule(query, rule);
            }

            var ids = await query.Select(p => p.Id).ToListAsync(ct);
            foreach (var id in ids) allMatchingIds.Add(id);
        }

        return allMatchingIds;
    }

    private IQueryable<Product> ApplyCollectionRule(IQueryable<Product> query, TaxonRule rule)
    {
        return rule.Type switch
        {
            "variant_price" => ApplyVariantPriceRule(query, rule),
            "variant_sku" => ApplyVariantSkuRule(query, rule),
            "product_property" => ApplyProductPropertyRule(query, rule),
            "classification_taxon" => ApplyClassificationTaxonRule(query, rule),
            _ => query.Where(p => false)
        };
    }

    private IQueryable<Product> ApplyClassificationTaxonRule(IQueryable<Product> query, TaxonRule rule)
    {
        if (!Guid.TryParse(rule.Value, out var targetTaxonId)) return query.Where(p => false);

        return rule.MatchPolicy switch
        {
            "is_equal_to" => query.Where(p => p.Classifications.Any(c => c.TaxonId == targetTaxonId)),
            "is_not_equal_to" => query.Where(p => !p.Classifications.Any(c => c.TaxonId == targetTaxonId)),
            _ => query.Where(p => false)
        };
    }

    private IQueryable<Product> ApplyVariantPriceRule(IQueryable<Product> query, TaxonRule rule)
    {
        if (!decimal.TryParse(rule.Value, out var price)) return query.Where(p => false);

        return rule.MatchPolicy switch
        {
            "is_equal_to" => query.Where(p => p.Variants.Any(v => v.Price == price)),
            "greater_than" => query.Where(p => p.Variants.Any(v => v.Price > price)),
            "less_than" => query.Where(p => p.Variants.Any(v => v.Price < price)),
            "greater_than_or_equal" => query.Where(p => p.Variants.Any(v => v.Price >= price)),
            "less_than_or_equal" => query.Where(p => p.Variants.Any(v => v.Price <= price)),
            _ => query.Where(p => false)
        };
    }

    private IQueryable<Product> ApplyVariantSkuRule(IQueryable<Product> query, TaxonRule rule)
    {
        return rule.MatchPolicy switch
        {
            "is_equal_to" => query.Where(p => p.Variants.Any(v => v.Sku == rule.Value)),
            "contains" => query.Where(p => p.Variants.Any(v => v.Sku != null && v.Sku.Contains(rule.Value))),
            "starts_with" => query.Where(p => p.Variants.Any(v => v.Sku != null && v.Sku.StartsWith(rule.Value))),
            "ends_with" => query.Where(p => p.Variants.Any(v => v.Sku != null && v.Sku.EndsWith(rule.Value))),
            _ => query.Where(p => false)
        };
    }

    private IQueryable<Product> ApplyProductPropertyRule(IQueryable<Product> query, TaxonRule rule)
    {
        if (string.IsNullOrEmpty(rule.PropertyName)) return query.Where(p => false);

        return rule.MatchPolicy switch
        {
            "is_equal_to" => query.Where(p => p.ProductProperties.Any(pp => pp.PropertyType.Name == rule.PropertyName && pp.Value == rule.Value)),
            "contains" => query.Where(p => p.ProductProperties.Any(pp => pp.PropertyType.Name == rule.PropertyName && pp.Value.Contains(rule.Value))),
            _ => query.Where(p => false)
        };
    }

    private async Task SyncClassificationsAsync(Taxon taxon, HashSet<Guid> matchingProductIds, CancellationToken ct)
    {
        var existingProductIds = taxon.Classifications.Select(c => c.ProductId).ToHashSet();

        var toAdd = matchingProductIds.Except(existingProductIds).ToList();
        var toRemoveIds = existingProductIds.Except(matchingProductIds).ToList();

        // Remove
        var classificationsToRemove = taxon.Classifications.Where(c => toRemoveIds.Contains(c.ProductId)).ToList();
        foreach (var c in classificationsToRemove)
        {
            taxon.Classifications.Remove(c);
            context.Set<Classification>().Remove(c);
        }

        // Add
        foreach (var productId in toAdd)
        {
            var classificationResult = Classification.Create(productId, taxon.Id);
            if (!classificationResult.IsError)
            {
                taxon.Classifications.Add(classificationResult.Value);
            }
        }

        // Mark products for regeneration if needed
        var affectedProductIds = toAdd.Concat(toRemoveIds).ToList();
        if (affectedProductIds.Any())
        {
            var products = await context.Set<Product>()
                .Where(p => affectedProductIds.Contains(p.Id))
                .ToListAsync(ct);

            foreach (var p in products) p.MarkedForRegenerateTaxonProducts = true;
        }

        await context.SaveChangesAsync(ct);
    }
}