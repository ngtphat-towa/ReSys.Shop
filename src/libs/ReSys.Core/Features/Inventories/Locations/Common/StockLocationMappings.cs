using Mapster;

using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.Features.Inventories.Locations.Common;

public class StockLocationMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StockLocation, StockLocationListItem>()
            .Map(dest => dest.Type, src => src.Type.ToString())
            .Map(dest => dest.City, src => src.Address.City)
            .Map(dest => dest.CountryCode, src => src.Address.CountryCode);

        config.NewConfig<StockLocation, StockLocationDetail>();

        // Ensure Address maps correctly between DTO and Domain
        config.NewConfig<AddressInput, Address>();
    }
}
