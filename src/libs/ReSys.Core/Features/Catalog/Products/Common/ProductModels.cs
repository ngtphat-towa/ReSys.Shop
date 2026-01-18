using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Features.Catalog.Products.Common;

// Parameters: Core fields only
public record ProductParameters
{
    public string Name { get; set; } = null!;
    public string Presentation { get; set; } = null!;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public DateTimeOffset? AvailableOn { get; set; }
    public DateTimeOffset? DiscontinuedOn { get; set; }
    public DateTimeOffset? MakeActiveAt { get; set; }
}

// Input: Includes Metadata, SKU, Price, SEO
public record ProductInput : ProductParameters, IHasMetadata
{
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

// Read model for Select List
public record ProductSelectListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}

// Read model for List
public record ProductListItem : ProductParameters
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
    public decimal Price { get; set; }
    public string Sku { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int VariantCount { get; set; }
}

// Read model for Detail
public record ProductDetail : ProductInput
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
}
