using ErrorOr;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Products;

/// <summary>
/// Joins a Product to a Taxon (Category) with positioning metadata.
/// Glue entity for the Catalog domain.
/// </summary>
public sealed class Classification : Aggregate
{
    public Guid ProductId { get; set; }
    public Guid TaxonId { get; set; }
    public int Position { get; set; }
    public bool IsAutomatic { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public Taxon Taxon { get; set; } = null!;

    public Classification() { }

    /// <summary>
    /// Factory for creating a product-taxon association.
    /// </summary>
    public static ErrorOr<Classification> Create(Guid productId, Guid taxonId, int position = 0, bool isAutomatic = false)
    {
        return new Classification
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            TaxonId = taxonId,
            Position = position,
            IsAutomatic = isAutomatic,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Updates the manual sort order of the product within the category.
    /// </summary>
    public void UpdatePosition(int position)
    {
        Position = position;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}