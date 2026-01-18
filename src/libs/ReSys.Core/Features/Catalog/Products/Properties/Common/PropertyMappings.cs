using Mapster;
using ReSys.Core.Domain.Catalog.Products.Properties;

namespace ReSys.Core.Features.Catalog.Products.Properties.Common;

public class PropertyMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProductProperty, ProductPropertyListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PropertyTypeId, src => src.PropertyTypeId)
            .Map(dest => dest.Value, src => src.Value)
            .Map(dest => dest.PropertyTypeName, src => src.PropertyType.Name)
            .Map(dest => dest.PropertyTypePresentation, src => src.PropertyType.Presentation);
    }
}
