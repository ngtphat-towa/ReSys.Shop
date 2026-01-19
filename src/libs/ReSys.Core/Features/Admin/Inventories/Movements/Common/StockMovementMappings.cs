using Mapster;
using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.Features.Admin.Inventories.Movements.Common;

public class StockMovementMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StockMovement, StockMovementListItem>()
            .Map(dest => dest.Sku, src => src.StockItem.Sku)
            .Map(dest => dest.Type, src => src.Type.ToString());
    }
}
