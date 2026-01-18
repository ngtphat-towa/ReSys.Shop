using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.Features.Inventories.Stocks.Common;

public record StockItemListItem
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public string Sku { get; set; } = null!;
    public string VariantName { get; set; } = null!;
    public Guid StockLocationId { get; set; }
    public string StockLocationName { get; set; } = null!;
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int CountAvailable { get; set; }
    public bool Backorderable { get; set; }
}

public record StockItemDetail : StockItemListItem
{
    public int BackorderLimit { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public record StockAdjustmentRequest
{
    public int Quantity { get; set; }
    public StockMovementType Type { get; set; }
    public decimal UnitCost { get; set; }
    public string? Reason { get; set; }
    public string? Reference { get; set; }
}

public record BackorderPolicyRequest
{
    public bool Backorderable { get; set; }
    public int BackorderLimit { get; set; }
}
