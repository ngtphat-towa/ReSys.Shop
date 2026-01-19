using Mapster;
using ReSys.Core.Domain.Ordering.InventoryUnits;

namespace ReSys.Core.Features.Admin.Inventories.Units.Common;

public class InventoryUnitMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventoryUnit, InventoryUnitListItem>()
            .Map(dest => dest.Sku, src => src.StockItem != null ? src.StockItem.Sku : string.Empty)
            .Map(dest => dest.State, src => src.State.ToString());

        config.NewConfig<InventoryUnit, InventoryUnitDetail>()
            .Map(dest => dest.Sku, src => src.StockItem != null ? src.StockItem.Sku : string.Empty)
            .Map(dest => dest.State, src => src.State.ToString())
            .Map(dest => dest.StockLocationName, src => src.StockItem != null && src.StockItem.StockLocation != null ? src.StockItem.StockLocation.Name : string.Empty);
    }
}
