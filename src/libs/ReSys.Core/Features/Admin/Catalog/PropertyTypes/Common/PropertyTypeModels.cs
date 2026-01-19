using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Features.Admin.Catalog.PropertyTypes.Common;

// Parameters: Core fields only
public record PropertyTypeParameters
{
    public string Name { get; init; } = null!;
    public string Presentation { get; init; } = null!;
    public PropertyKind Kind { get; init; }
    public int Position { get; init; }
    public bool Filterable { get; init; }
}

// Input: Includes Metadata
public record PropertyTypeInput : PropertyTypeParameters, IHasMetadata
{
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

// Read model for Select List: No metadata needed
public record PropertyTypeSelectListItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
}

// Read model for List: Includes Metadata
public record PropertyTypeListItem : PropertyTypeParameters, IHasMetadata
{
    public Guid Id { get; init; }
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

// Read model for Detail: Includes Metadata
public record PropertyTypeDetail : PropertyTypeParameters, IHasMetadata
{
    public Guid Id { get; init; }
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}
