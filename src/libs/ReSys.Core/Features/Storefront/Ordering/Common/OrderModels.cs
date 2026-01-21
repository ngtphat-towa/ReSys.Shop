using ReSys.Core.Domain.Ordering;

namespace ReSys.Core.Features.Storefront.Ordering.Common;

public record OrderSummary
{
    public Guid Id { get; set; }
    public string Number { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal TotalCents { get; set; }
    public int ItemCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public record LineItemResponse
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public long PriceCents { get; set; }
    public long TotalCents { get; set; }
    public string? ImageUrl { get; set; }
}

public record OrderDetailResponse : OrderSummary
{
    public List<LineItemResponse> Items { get; set; } = [];
    public decimal ItemTotalCents { get; set; }
    public decimal ShipmentTotalCents { get; set; }
    public decimal AdjustmentTotalCents { get; set; }
    
    // Address info
    public Guid? ShipAddressId { get; set; }
    public Guid? BillAddressId { get; set; }
    
    // Shipping
    public Guid? ShippingMethodId { get; set; }
}

public record CartResponse : OrderDetailResponse
{
}

public record SetCheckoutAddressesRequest(Guid ShippingAddressId, Guid BillingAddressId);

public record SetShippingMethodRequest(Guid ShippingMethodId);

public record PlaceOrderRequest(Guid PaymentMethodId);

public record OrderPlacementResult(string ClientSecret, string TransactionId);

