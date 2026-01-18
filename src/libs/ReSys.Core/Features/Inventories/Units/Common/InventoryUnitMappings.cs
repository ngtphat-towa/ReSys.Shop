using Mapster;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Inventories.Units.Common;

public class InventoryUnitMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventoryUnit, InventoryUnitListItem>()
            .Map(dest => dest.Sku, src => src.StockItem.Sku)
            .Map(dest => dest.State, src => src.State.ToString());

        config.NewConfig<InventoryUnit, InventoryUnitDetail>()
            .Map(dest => dest.Sku, src => src.StockItem.Sku)
            .Map(dest => dest.State, src => src.State.ToString())
            .Map(dest => dest.StockLocationName, src => src.StockItem.StockLocation.Name);
    }
}
