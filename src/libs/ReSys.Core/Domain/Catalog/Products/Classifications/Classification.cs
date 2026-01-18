using ErrorOr;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Products;

public sealed class Classification : Aggregate
{
    public Guid ProductId { get; private set; }
    public Guid TaxonId { get; private set; }
    public int Position { get; private set; }
    public bool IsAutomatic { get; private set; }

    // Navigation
    public Product Product { get; private set; } = null!;
    public Taxon Taxon { get; private set; } = null!;

    private Classification() { }

    public static ErrorOr<Classification> Create(Guid productId, Guid taxonId, int position = 0, bool isAutomatic = false)
    {
        return new Classification
        {
            ProductId = productId,
            TaxonId = taxonId,
            Position = position,
            IsAutomatic = isAutomatic
        };
    }

    public void UpdatePosition(int position)
    {
        Position = position;
    }
}