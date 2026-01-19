using ReSys.Core.Domain.Catalog.Products.Images;

namespace ReSys.Core.Features.Admin.Catalog.Products.Images.Common;

public record ProductImageParameters
{
    public string? Alt { get; set; }
    public int Position { get; set; }
    public ProductImage.ProductImageType Role { get; set; }
}

public record ProductImageListItem : ProductImageParameters
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public ProductImage.ImageProcessingStatus Status { get; set; }
}
