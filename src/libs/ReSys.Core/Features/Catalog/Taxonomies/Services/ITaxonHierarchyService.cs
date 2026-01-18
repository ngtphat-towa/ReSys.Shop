using ErrorOr;

using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Catalog.Taxonomies.Services;

/// <summary>
/// Provides services for managing and querying the hierarchical structure of taxons within taxonomies.
/// </summary>
public interface ITaxonHierarchyService
{
    /// <summary>
    /// Rebuilds the entire hierarchy for a taxonomy, including nested sets and permalinks.
    /// </summary>
    /// <param name="taxonomyId">The unique identifier of the taxonomy to rebuild.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<ErrorOr<Success>> RebuildHierarchyAsync(Guid taxonomyId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the hierarchy of a taxonomy for cycles and multiple roots.
    /// </summary>
    /// <param name="taxonomyId">The unique identifier of the taxonomy to validate.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<ErrorOr<Success>> ValidateHierarchyAsync(Guid taxonomyId, CancellationToken cancellationToken);

    /// <summary>
    /// Rebuilds the nested set model (Lft, Rgt values) for a taxonomy.
    /// </summary>
    /// <param name="taxonomyId">The unique identifier of the taxonomy.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<ErrorOr<Success>> RebuildNestedSetsAsync(Guid taxonomyId, CancellationToken cancellationToken);

    /// <summary>
    /// Regenerates permalinks and pretty names for all taxons in a taxonomy based on the current structure.
    /// </summary>
    /// <param name="taxonomyId">The unique identifier of the taxonomy.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<ErrorOr<Success>> RegeneratePermalinksAsync(Guid taxonomyId, CancellationToken cancellationToken);

    /// <summary>
    /// Builds a hierarchical tree structure of taxons based on the provided options.
    /// </summary>
    /// <param name="options">Query options for filtering and focusing the tree.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A response containing the tree structure and metadata.</returns>
    Task<TaxonTreeResponse> BuildTaxonTreeAsync(TaxonQueryOptions options, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated flat list of taxons based on the provided options.
    /// </summary>
    /// <param name="options">Query options for filtering, searching, and pagination.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A paged list of taxon items.</returns>
    Task<PagedList<TaxonListItem>> GetFlatTaxonsAsync(TaxonQueryOptions options, CancellationToken cancellationToken);
}