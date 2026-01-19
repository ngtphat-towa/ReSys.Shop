namespace ReSys.Core.Features.Ordering.Common;

public record OrderSummaryResponse
{
    public Guid Id { get; init; }
    public string Number { get; init; } = null!;
    public string State { get; init; } = null!;
    public string Currency { get; init; } = null!;
    
    public long TotalCents { get; init; }
    public string TotalDisplay { get; init; } = null!;
    
    public string? Email { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record OrderDetailResponse : OrderSummaryResponse
{
    public long ItemTotalCents { get; init; }
    public string ItemTotalDisplay { get; init; } = null!;
    
    public long ShipmentTotalCents { get; init; }
    public string ShipmentTotalDisplay { get; init; } = null!;
    
    public long AdjustmentTotalCents { get; init; }
    public string AdjustmentTotalDisplay { get; init; } = null!;

    public List<LineItemResponse> LineItems { get; init; } = [];
    public List<OrderAdjustmentResponse> Adjustments { get; init; } = [];
    public List<PaymentResponse> Payments { get; init; } = [];
    public List<ShipmentResponse> Shipments { get; init; } = [];
    public List<OrderHistoryResponse> History { get; init; } = [];
}

public record LineItemResponse
{
    public Guid Id { get; init; }
    public Guid VariantId { get; init; }
    public string Name { get; init; } = null!;
    public string SKU { get; init; } = null!;
    public int Quantity { get; init; }
    
    public long UnitPriceCents { get; init; }
    public string UnitPriceDisplay { get; init; } = null!;
    
    public long TotalCents { get; init; }
    public string TotalDisplay { get; init; } = null!;
}

public record OrderAdjustmentResponse
{
    public Guid Id { get; init; }
    public string Description { get; init; } = null!;
    
    public long AmountCents { get; init; }
    public string AmountDisplay { get; init; } = null!;
    
    public string Scope { get; init; } = null!;
    public bool IsPromotion { get; init; }
}

public record PaymentResponse
{
    public Guid Id { get; init; }
    
    public long AmountCents { get; init; }
    public string AmountDisplay { get; init; } = null!;
    
    public string State { get; init; } = null!;
    public string MethodType { get; init; } = null!;
    public string? TransactionId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record ShipmentResponse
{
    public Guid Id { get; init; }
    public string Number { get; init; } = null!;
    public string State { get; init; } = null!;
    public string? TrackingNumber { get; init; }
    public Guid StockLocationId { get; init; }
    public string? StockLocationName { get; init; }
    public List<InventoryUnitResponse> Units { get; init; } = [];
}

public record InventoryUnitResponse
{
    public Guid Id { get; init; }
    public string SKU { get; init; } = null!;
    public string State { get; init; } = null!;
    public string? SerialNumber { get; init; }
    public bool Pending { get; init; }
}

public record OrderHistoryResponse
{
    public string Description { get; init; } = null!;
    public string? FromState { get; init; }
    public string ToState { get; init; } = null!;
    public string? TriggeredBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IDictionary<string, object?> Context { get; init; } = new Dictionary<string, object?>();
}