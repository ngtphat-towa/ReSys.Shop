using Mapster;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Admin.Inventories.Stocks.Common;

public class StockItemMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StockItem, StockItemListItem>()
            .Map(dest => dest.VariantName, src => src.Variant.Product.Name)
            .Map(dest => dest.StockLocationName, src => src.StockLocation.Name);

        config.NewConfig<StockItem, StockItemDetail>()
            .Map(dest => dest.VariantName, src => src.Variant.Product.Name)
            .Map(dest => dest.StockLocationName, src => src.StockLocation.Name);
    }
}
