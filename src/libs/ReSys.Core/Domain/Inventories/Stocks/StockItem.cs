using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Stocks;

public sealed class StockItem : Aggregate, IHasMetadata
{
    public Guid VariantId { get; private set; }
    public Guid StockLocationId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }
    public bool Backorderable { get; private set; } = true;

    public IDictionary<string, object?> PublicMetadata { get; private set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; private set; } = new Dictionary<string, object?>();

    public int CountAvailable => Math.Max(0, QuantityOnHand - QuantityReserved);

    private StockItem() { }

    public static ErrorOr<StockItem> Create(Guid variantId, Guid stockLocationId, string sku, int initialStock = 0)
    {
        if (initialStock < 0) return StockItemErrors.InvalidQuantity;

        var item = new StockItem
        {
            VariantId = variantId,
            StockLocationId = stockLocationId,
            Sku = sku,
            QuantityOnHand = initialStock
        };

        item.RaiseDomainEvent(new StockItemEvents.StockItemCreated(item));
        return item;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0) return;
        QuantityOnHand += quantity;
        RaiseDomainEvent(new StockItemEvents.StockRestocked(this, quantity));
    }

    public ErrorOr<Success> Reserve(int quantity)
    {
        if (quantity <= 0) return StockItemErrors.InvalidQuantity;
        if (!Backorderable && CountAvailable < quantity) return StockItemErrors.InsufficientStock;

        QuantityReserved += quantity;
        RaiseDomainEvent(new StockItemEvents.StockReserved(this, quantity));
        return Result.Success;
    }

    public ErrorOr<Success> Fulfill(int quantity)
    {
        if (quantity <= 0) return StockItemErrors.InvalidQuantity;
        if (QuantityReserved < quantity) return StockItemErrors.InvalidRelease;

        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
        
        RaiseDomainEvent(new StockItemEvents.StockFilled(this, quantity));
        return Result.Success;
    }
}
