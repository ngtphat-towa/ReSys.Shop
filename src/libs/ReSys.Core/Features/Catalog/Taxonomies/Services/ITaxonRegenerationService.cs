using ErrorOr;

namespace ReSys.Core.Features.Catalog.Taxonomies.Services;

/// <summary>
/// Provides services for automatically classifying products into taxons based on defined rules.
/// </summary>
public interface ITaxonRegenerationService
{
    /// <summary>
    /// Regenerates the product classifications for a specific taxon by evaluating its rules against all active products.
    /// </summary>
    /// <param name="taxonId">The unique identifier of the taxon to regenerate.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<ErrorOr<Success>> RegenerateProductsForTaxonAsync(Guid taxonId, CancellationToken cancellationToken);
}