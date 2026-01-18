using Mapster;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Features.Catalog.Products.Variants.Common;

public class VariantMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Variant, VariantListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.ProductName, src => src.Product.Name)
            .Map(dest => dest.Options, src => src.OptionValues.Select(ov => new VariantOptionModel
            {
                Name = ov.OptionType.Presentation,
                Value = ov.Presentation
            }));

        config.NewConfig<Variant, VariantDetail>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.ProductName, src => src.Product.Name)
            .Map(dest => dest.Options, src => src.OptionValues.Select(ov => new VariantOptionModel
            {
                Name = ov.OptionType.Presentation,
                Value = ov.Presentation
            }));

        config.NewConfig<Variant, VariantSelectListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Sku, src => src.Sku)
            .Map(dest => dest.ProductName, src => src.Product.Name);
    }
}
