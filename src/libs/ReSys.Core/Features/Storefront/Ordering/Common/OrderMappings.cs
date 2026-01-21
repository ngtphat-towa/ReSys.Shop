using Mapster;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.LineItems;
using ReSys.Core.Features.Storefront.Ordering.Common;

namespace ReSys.Core.Features.Storefront.Ordering.Common;

public class OrderMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Order, OrderSummary>()
            .Map(dest => dest.State, src => src.State.ToString())
            .Map(dest => dest.ItemCount, src => src.LineItems.Sum(li => li.Quantity));

        config.NewConfig<LineItem, LineItemResponse>()
            .Map(dest => dest.ProductName, src => src.CapturedName)
            .Map(dest => dest.Sku, src => src.CapturedSku)
            .Map(dest => dest.TotalCents, src => src.GetTotalCents())
            .Map(dest => dest.ImageUrl, src => src.Variant.Product != null && src.Variant.Product.Images.Any()
                ? (src.Variant.Product.Images.FirstOrDefault(i => i.Role == ReSys.Core.Domain.Catalog.Products.Images.ProductImage.ProductImageType.Default) ?? src.Variant.Product.Images.First()).Url 
                : null);

        config.NewConfig<Order, OrderDetailResponse>()
            .Inherits<Order, OrderSummary>()
            .Map(dest => dest.Items, src => src.LineItems);

        config.NewConfig<Order, CartResponse>()
            .Inherits<Order, OrderDetailResponse>();
    }
}
