using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.Features.Inventories.Movements.Common;

public record StockMovementListItem
{
    public Guid Id { get; set; }
    public Guid StockItemId { get; set; }
    public string Sku { get; set; } = null!;
    public int Quantity { get; set; }
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    public string Type { get; set; } = null!;
    public string? Reason { get; set; }
    public string? Reference { get; set; }
    public decimal UnitCost { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
