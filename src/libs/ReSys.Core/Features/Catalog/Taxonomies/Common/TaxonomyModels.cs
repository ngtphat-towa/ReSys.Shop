using System.Linq.Expressions;

using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Common;

// Parameters: Core fields only
public record TaxonomyParameters
{
    public string Name { get; set; } = null!;
    public string Presentation { get; set; } = null!;
    public int Position { get; set; }
}

// Input: Includes Metadata
public record TaxonomyInput : TaxonomyParameters, IHasMetadata
{
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

// Read model for Select List:
public record TaxonomySelectListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

// Read model for List: Includes Metadata
public record TaxonomyListItem : TaxonomyParameters
{
    public Guid Id { get; set; }
    public int TaxonCount { get; set; }
}

// Read model for Detail: Includes Metadata and Root Taxons
public record TaxonomyDetail : TaxonomyParameters, IHasMetadata
{
    public Guid Id { get; set; }
    public int TaxonCount { get; set; }
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public IEnumerable<TaxonListItem> RootTaxons { get; set; } = [];
}