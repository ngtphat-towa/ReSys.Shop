using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;

/// <summary>
/// Service responsible for evaluating Taxon Rules against Products.
/// </summary>
public interface ITaxonEvaluator
{
    /// <summary>
    /// Filters a product query based on the automatic rules defined in a Taxon.
    /// </summary>
    /// <param name="query">The base product query.</param>
    /// <param name="taxon">The taxon containing rules to apply.</param>
    /// <returns>A filtered queryable of products.</returns>
    IQueryable<Product> ApplyTaxonRules(IQueryable<Product> query, Taxon taxon);
}
