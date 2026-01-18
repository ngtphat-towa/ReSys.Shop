using Mapster;
using ReSys.Core.Domain.Catalog.Products.Images;

namespace ReSys.Core.Features.Catalog.Products.Images.Common;

public class ImageMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProductImage, ProductImageListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Url, src => src.Url)
            .Map(dest => dest.Alt, src => src.Alt)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.Role, src => src.Role)
            .Map(dest => dest.Status, src => src.Status);
    }
}
