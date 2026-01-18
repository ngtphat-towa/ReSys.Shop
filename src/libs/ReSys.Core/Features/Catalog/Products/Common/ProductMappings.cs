using Mapster;

using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Images;

namespace ReSys.Core.Features.Catalog.Products.Common;

public class ProductMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Slug, src => src.Slug)
            .Map(dest => dest.AvailableOn, src => src.AvailableOn)
            .Map(dest => dest.DiscontinuedOn, src => src.DiscontinuedOn)
            .Map(dest => dest.MakeActiveAt, src => src.MakeActiveAt)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Price, src => src.MasterVariant != null ? src.MasterVariant.Price : 0)
            .Map(dest => dest.Sku, src => src.MasterVariant != null ? src.MasterVariant.Sku : string.Empty)
            .Map(dest => dest.VariantCount, src => src.Variants.Count)
            .Map(dest => dest.ImageUrl, src => src.Images.Where(i => i.Role == ProductImage.ProductImageType.Default).Select(i => i.Url).FirstOrDefault()
                                               ?? src.Images.OrderBy(i => i.Position).Select(i => i.Url).FirstOrDefault());

        config.NewConfig<Product, ProductDetail>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Slug, src => src.Slug)
            .Map(dest => dest.AvailableOn, src => src.AvailableOn)
            .Map(dest => dest.DiscontinuedOn, src => src.DiscontinuedOn)
            .Map(dest => dest.MakeActiveAt, src => src.MakeActiveAt)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Sku, src => src.MasterVariant != null ? src.MasterVariant.Sku : string.Empty)
            .Map(dest => dest.Price, src => src.MasterVariant != null ? src.MasterVariant.Price : 0)
            .Map(dest => dest.MetaTitle, src => src.MetaTitle)
            .Map(dest => dest.MetaDescription, src => src.MetaDescription)
            .Map(dest => dest.MetaKeywords, src => src.MetaKeywords)
            .Map(dest => dest.PublicMetadata, src => src.PublicMetadata)
            .Map(dest => dest.PrivateMetadata, src => src.PrivateMetadata);

        config.NewConfig<Product, ProductSelectListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Slug, src => src.Slug);
    }
}
