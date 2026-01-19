using Mapster;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Features.Admin.Inventories.Transfers.Common;

public class StockTransferMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // To satisfy location names, we'll need to join in the handler or use Mapster's after-mapping
        // For simplicity here, we map the status
        config.NewConfig<StockTransfer, StockTransferListItem>()
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<StockTransfer, StockTransferDetail>()
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<StockTransferItem, StockTransferItemModel>()
            .Map(dest => dest.VariantName, src => "N/A"); // Logic handled by projecting joined query
    }
}
