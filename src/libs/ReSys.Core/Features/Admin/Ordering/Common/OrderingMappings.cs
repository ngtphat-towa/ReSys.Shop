using Mapster;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.LineItems;
using ReSys.Core.Domain.Ordering.Adjustments;
using ReSys.Core.Domain.Ordering.Payments;
using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Core.Domain.Ordering.History;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using System.Globalization;

namespace ReSys.Core.Features.Admin.Ordering.Common;

public class OrderingMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 1. Order Summary & Detail
        config.NewConfig<Order, OrderSummaryResponse>()
            .Map(dest => dest.TotalDisplay, src => FormatCents(src.TotalCents, src.Currency));

        config.NewConfig<Order, OrderDetailResponse>()
            .Inherits<Order, OrderSummaryResponse>()
            .Map(dest => dest.ItemTotalDisplay, src => FormatCents(src.ItemTotalCents, src.Currency))
            .Map(dest => dest.ShipmentTotalDisplay, src => FormatCents(src.ShipmentTotalCents, src.Currency))
            .Map(dest => dest.AdjustmentTotalDisplay, src => FormatCents(src.AdjustmentTotalCents, src.Currency))
            .Map(dest => dest.History, src => src.Histories);

        // 2. Line Items
        config.NewConfig<LineItem, LineItemResponse>()
            .Map(dest => dest.SKU, src => src.CapturedSku ?? string.Empty)
            .Map(dest => dest.Name, src => src.CapturedName)
            .Map(dest => dest.UnitPriceCents, src => src.PriceCents)
            .Map(dest => dest.UnitPriceDisplay, src => FormatCents(src.PriceCents, src.Currency))
            .Map(dest => dest.TotalCents, src => src.GetTotalCents())
            .Map(dest => dest.TotalDisplay, src => FormatCents(src.GetTotalCents(), src.Currency));

        // 3. Adjustments
        config.NewConfig<OrderAdjustment, OrderAdjustmentResponse>()
            .Map(dest => dest.AmountDisplay, src => FormatCents(src.AmountCents, src.Order.Currency));

        // 4. Payments
        config.NewConfig<Payment, PaymentResponse>()
            .Map(dest => dest.AmountDisplay, src => FormatCents(src.AmountCents, src.Currency))
            .Map(dest => dest.MethodType, src => src.PaymentMethodType);

        // 5. Shipments & Units
        config.NewConfig<Shipment, ShipmentResponse>()
            .Map(dest => dest.Units, src => src.InventoryUnits);

        config.NewConfig<InventoryUnit, InventoryUnitResponse>()
            .Map(dest => dest.SKU, src => src.StockItem != null ? src.StockItem.Sku : "Unknown");

        // 6. History
        config.NewConfig<OrderHistory, OrderHistoryResponse>()
            .Map(dest => dest.FromState, src => src.FromState.HasValue ? src.FromState.Value.ToString() : null)
            .Map(dest => dest.ToState, src => src.ToState.ToString());
    }

    private static string FormatCents(long cents, string currencyCode)
    {
        var amount = cents / 100m;
        // Basic formatting, could be enhanced with culture-specific logic based on currencyCode
        return amount.ToString("C2", CultureInfo.CreateSpecificCulture("en-US"));
    }
}
