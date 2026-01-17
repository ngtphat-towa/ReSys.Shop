using Mapster;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;

namespace ReSys.Core.Features.Catalog.PropertyTypes.Common;

public class PropertyTypeMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 1. PropertyType -> PropertyTypeSelectListItem
        config.NewConfig<PropertyType, PropertyTypeSelectListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        // 2. PropertyType -> PropertyTypeListItem
        config.NewConfig<PropertyType, PropertyTypeListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Kind, src => src.Kind)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.Filterable, src => src.Filterable)
            .Map(dest => dest.PublicMetadata, src => src.PublicMetadata ?? new Dictionary<string, object?>())
            .Map(dest => dest.PrivateMetadata, src => src.PrivateMetadata ?? new Dictionary<string, object?>());

        // 3. PropertyType -> PropertyTypeDetail
        config.NewConfig<PropertyType, PropertyTypeDetail>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Kind, src => src.Kind)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.Filterable, src => src.Filterable)
            .Map(dest => dest.PublicMetadata, src => src.PublicMetadata ?? new Dictionary<string, object?>())
            .Map(dest => dest.PrivateMetadata, src => src.PrivateMetadata ?? new Dictionary<string, object?>());

        // 4. Inherit for specific slice responses
        config.NewConfig<PropertyType, GetPropertyTypesPagedList.GetPropertyTypesPagedList.Response>()
            .Inherits<PropertyType, PropertyTypeListItem>();

        config.NewConfig<PropertyType, GetPropertyTypeSelectList.GetPropertyTypeSelectList.Response>()
            .Inherits<PropertyType, PropertyTypeSelectListItem>();

        config.NewConfig<PropertyType, GetPropertyTypeDetail.GetPropertyTypeDetail.Response>()
            .Inherits<PropertyType, PropertyTypeDetail>();

        config.NewConfig<PropertyType, CreatePropertyType.CreatePropertyType.Response>()
            .Inherits<PropertyType, PropertyTypeDetail>();

        config.NewConfig<PropertyType, UpdatePropertyType.UpdatePropertyType.Response>()
            .Inherits<PropertyType, PropertyTypeDetail>();
    }
}
