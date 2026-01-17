using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;

public class OptionValueMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 1. OptionValue -> OptionValueModel
        config.NewConfig<OptionValue, OptionValueModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Position, src => src.Position);

        // 2. Inherit for specific slice responses
        config.NewConfig<OptionValue, CreateOptionValue.CreateOptionValue.Response>()
            .Inherits<OptionValue, OptionValueModel>();

        config.NewConfig<OptionValue, UpdateOptionValue.UpdateOptionValue.Response>()
            .Inherits<OptionValue, OptionValueModel>();
    }
}
