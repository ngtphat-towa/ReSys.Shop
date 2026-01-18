using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes;

namespace ReSys.Core.Features.Catalog.OptionTypes.Common;

public class OptionTypeMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 1. OptionType -> OptionTypeSelectListItem
        config.NewConfig<OptionType, OptionTypeSelectListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        // 2. OptionType -> OptionTypeListItem
        config.NewConfig<OptionType, OptionTypeListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.Filterable, src => src.Filterable)
            .Map(dest => dest.PublicMetadata, src => src.PublicMetadata)
            .Map(dest => dest.PrivateMetadata, src => src.PrivateMetadata);

        // 3. OptionType -> OptionTypeDetail
        config.NewConfig<OptionType, OptionTypeDetail>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.Filterable, src => src.Filterable)
            .Map(dest => dest.PublicMetadata, src => src.PublicMetadata)
            .Map(dest => dest.PrivateMetadata, src => src.PrivateMetadata)
            .Map(dest => dest.OptionValues, src => src.OptionValues
                .OrderBy(ov => ov.Position));

        // 4. Inherit for specific slice responses
        config.NewConfig<OptionType, GetOptionTypesPagedList.GetOptionTypesPagedList.Response>()
            .Inherits<OptionType, OptionTypeListItem>();

        config.NewConfig<OptionType, GetOptionTypeSelectList.GetOptionTypeSelectList.Response>()
            .Inherits<OptionType, OptionTypeSelectListItem>();

        config.NewConfig<OptionType, GetOptionTypeDetail.GetOptionTypeDetail.Response>()
            .Inherits<OptionType, OptionTypeDetail>();

        config.NewConfig<OptionType, CreateOptionType.CreateOptionType.Response>()
            .Inherits<OptionType, OptionTypeDetail>();

        config.NewConfig<OptionType, UpdateOptionType.UpdateOptionType.Response>()
            .Inherits<OptionType, OptionTypeDetail>();
    }
}