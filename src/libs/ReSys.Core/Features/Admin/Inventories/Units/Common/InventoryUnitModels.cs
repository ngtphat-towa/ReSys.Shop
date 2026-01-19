using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Admin.Inventories.Units.Common;

public record InventoryUnitListItem
{
    public Guid Id { get; set; }
    public Guid StockItemId { get; set; }
    public string Sku { get; set; } = null!;
    public string State { get; set; } = null!;
    public Guid? OrderId { get; set; }
    public string? SerialNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public record InventoryUnitDetail : InventoryUnitListItem
{
    public Guid VariantId { get; set; }
    public Guid? StockLocationId { get; set; }
    public string? StockLocationName { get; set; }
    public Guid? ShipmentId { get; set; }
    public Guid? LineItemId { get; set; }
    public string? LotNumber { get; set; }
}

public record UpdateSerialNumberRequest(string SerialNumber);
