using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Features.Admin.Catalog.Products.Variants.Common;

public record VariantParameters
{
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    
    // Physical
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
    
    // Pricing
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    
    public bool TrackInventory { get; set; } = true;
    public int Position { get; set; }
}

public record VariantInput : VariantParameters, IHasMetadata
{
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

public record VariantOptionModel
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public record VariantListItem : VariantParameters
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public bool IsMaster { get; set; }
    public List<VariantOptionModel> Options { get; set; } = new();
}

public record VariantDetail : VariantInput
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public bool IsMaster { get; set; }
    public List<VariantOptionModel> Options { get; set; } = new();
}

public record VariantSelectListItem
{
    public Guid Id { get; set; }
    public string? Sku { get; set; }
    public string ProductName { get; set; } = string.Empty;
}
