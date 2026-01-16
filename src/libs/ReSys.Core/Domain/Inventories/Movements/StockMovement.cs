using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Inventories.Movements;

public sealed class StockMovement : Entity, IAuditable
{
    public Guid StockItemId { get; private set; }
    public int Quantity { get; private set; }
    public string? Action { get; private set; }
    public string? Reason { get; private set; }

    // IAuditable implementation is inherited from Entity

    private StockMovement() { }

    public static StockMovement Create(Guid stockItemId, int quantity, string action, string? reason = null)
    {
        return new StockMovement
        {
            StockItemId = stockItemId,
            Quantity = quantity,
            Action = action,
            Reason = reason
        };
    }
}