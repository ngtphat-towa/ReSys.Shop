using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;

// Parameters: Core fields only
public record OptionTypeParameters
{
    public string Name { get; init; } = null!;
    public string Presentation { get; init; } = null!;
    public string? Description { get; init; }
    public int Position { get; init; }
    public bool Filterable { get; init; }
}

// Input: Includes Metadata
public record OptionTypeInput : OptionTypeParameters, IHasMetadata
{
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

// Read model for Select List: No metadata
public record OptionTypeSelectListItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
}

// Read model for List: Includes Metadata
public record OptionTypeListItem : OptionTypeParameters, IHasMetadata
{
    public Guid Id { get; init; }
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

// Read model for Detail: Includes Metadata and Values
public record OptionTypeDetail : OptionTypeListItem
{
    public IEnumerable<OptionValueModel> OptionValues { get; init; } = [];
}
