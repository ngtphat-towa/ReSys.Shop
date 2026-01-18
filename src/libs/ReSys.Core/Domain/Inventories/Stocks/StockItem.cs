using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Domain.Inventories.Stocks;

/// <summary>
/// Represents the physical and logical inventory of a product variant at a specific location.
/// Orchestrates the balance between physical stock, customer reservations, and the audit ledger.
/// </summary>
public sealed class StockItem : Aggregate, IHasMetadata, ISoftDeletable
{
    public Guid VariantId { get; private set; }
    public Guid StockLocationId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }
    public bool Backorderable { get; private set; } = true;
    public int BackorderLimit { get; private set; } = StockItemConstraints.DefaultMaxBackorderQuantity;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation
    public Variant Variant { get; private set; } = null!;
    public StockLocation StockLocation { get; private set; } = null!;

    public IDictionary<string, object?> PublicMetadata { get; private set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; private set; } = new Dictionary<string, object?>();

    public ICollection<StockMovement> StockMovements { get; private set; } = new List<StockMovement>();
    public ICollection<InventoryUnit> InventoryUnits { get; private set; } = new List<InventoryUnit>();

    /// <summary>
    /// Logic: Real-time availability calculation. 
    /// If backorderable, we show the true potential. If not, we never show less than zero.
    /// </summary>
    public int CountAvailable => Backorderable
        ? (QuantityOnHand - QuantityReserved)
        : Math.Max(0, QuantityOnHand - QuantityReserved);

    private StockItem() { }

    /// <summary>
    /// Creates a new stock item record and initializes the audit ledger.
    /// </summary>
    public static ErrorOr<StockItem> Create(Guid variantId, Guid stockLocationId, string sku, int initialStock = 0, decimal initialUnitCost = 0)
    {
        // Guard: Prevent extreme or impossible quantities
        if (initialStock < StockItemConstraints.MinQuantity || initialStock > StockItemConstraints.MaxQuantity)
            return StockItemErrors.InvalidQuantity(StockItemConstraints.MinQuantity, StockItemConstraints.MaxQuantity);

        // Guard: Ensure traceability via SKU
        if (string.IsNullOrWhiteSpace(sku)) return StockItemErrors.SkuRequired;

        var item = new StockItem
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            StockLocationId = stockLocationId,
            Sku = sku.Trim(),
            QuantityOnHand = initialStock
        };

        // Business Rule: Every initial balance must have a corresponding ledger entry
        if (initialStock > 0)
        {
            var initialMovement = StockMovement.Create(
                item.Id,
                initialStock,
                0,
                StockMovementType.Receipt,
                initialUnitCost,
                "Initial inventory creation");

            item.StockMovements.Add(initialMovement);
        }

        item.RaiseDomainEvent(new StockItemEvents.StockItemCreated(item));
        return item;
    }

    /// <summary>
    /// Adjusts physical stock (Receipts, Losses, Audits) and promotes waiting backorders.
    /// </summary>
    public ErrorOr<Success> AdjustStock(int quantity, StockMovementType type, decimal unitCost = 0, string? reason = null, string? reference = null)
    {
        // Guard: Prevent noise in the ledger
        if (quantity == 0) return StockItemErrors.ZeroQuantityMovement;

        var balanceBefore = QuantityOnHand;
        var newQuantity = QuantityOnHand + quantity;

        // Guard: Prevent overflow/underflow based on system constraints
        if (newQuantity < StockItemConstraints.MinQuantity || newQuantity > StockItemConstraints.MaxQuantity)
            return StockItemErrors.InvalidQuantity(StockItemConstraints.MinQuantity, StockItemConstraints.MaxQuantity);

        // Guard: Prevent exceeding the backorder floor (The risk limit)
        if (Backorderable && newQuantity < -BackorderLimit)
            return StockItemErrors.BackorderLimitExceeded(BackorderLimit, newQuantity);

        // Guard: Prevent negative numbers if the shop policy blocks backordering
        if (!Backorderable && newQuantity < 0)
            return StockItemErrors.InsufficientStock(CountAvailable, Math.Abs(quantity));

        QuantityOnHand = newQuantity;

        // Business Rule: Backorder Backfilling
        // If we added physical stock, we must prioritize existing customer debt (Backorders)
        if (quantity > 0)
        {
            var backorderedUnits = InventoryUnits
                .Where(u => u.State == InventoryUnitState.Backordered)
                .OrderBy(u => u.CreatedAt) // First-come, first-served
                .Take(quantity)
                .ToList();

            foreach (var unit in backorderedUnits)
            {
                // Transition: Promote from Backordered to OnHand (physical earmark)
                var result = unit.Reserve(unit.OrderId!.Value);
                if (!result.IsError)
                {
                    QuantityReserved++;
                }
            }
        }

        // Business Rule: Record the physical truth in the Ledger
        var movement = StockMovement.Create(Id, quantity, balanceBefore, type, unitCost, reason, reference);
        StockMovements.Add(movement);

        RaiseDomainEvent(new StockItemEvents.StockAdjusted(this, quantity, type, reference));
        return Result.Success;
    }

    /// <summary>
    /// Promises physical stock to an order. Does not change physical quantity until fulfillment.
    /// </summary>
    public ErrorOr<Success> Reserve(int quantity, Guid orderId, Guid lineItemId)
    {
        // Guard: Prevent empty or negative reservations
        if (quantity <= 0) return StockItemErrors.InvalidQuantity(1, int.MaxValue);

        // Guard: Ensure availability if backordering is disabled
        if (!Backorderable && CountAvailable < quantity)
            return StockItemErrors.InsufficientStock(CountAvailable, quantity);

        for (int i = 0; i < quantity; i++)
        {
            var unit = InventoryUnit.Create(Id, VariantId, lineItemId, StockLocationId, InventoryUnitState.Pending);

            // Logic: Determine if this unit is a physical earmark (OnHand) or a promise (Backordered)
            if (CountAvailable > 0)
            {
                unit.Reserve(orderId);
                QuantityReserved++;
            }
            else
            {
                unit.Backorder(orderId);
            }

            InventoryUnits.Add(unit);
        }

        RaiseDomainEvent(new StockItemEvents.StockReserved(this, quantity, orderId.ToString()));
        return Result.Success;
    }

    /// <summary>
    /// Releases reserved stock back to availability (e.g. order cancellation).
    /// </summary>
    public ErrorOr<Success> Release(int quantity, Guid orderId)
    {
        // Guard: Ensure non-zero release
        if (quantity <= 0) return StockItemErrors.InvalidQuantity(1, int.MaxValue);

        var unitsToRelease = InventoryUnits
            .Where(u => u.OrderId == orderId && (u.State == InventoryUnitState.OnHand || u.State == InventoryUnitState.Backordered))
            .Take(quantity)
            .ToList();

        // Guard: Prevent over-releasing
        if (unitsToRelease.Count < quantity)
            return StockItemErrors.InvalidRelease(unitsToRelease.Count, quantity);

        foreach (var unit in unitsToRelease)
        {
            // Business Rule: Only physical earmarks decrement the reserved counter
            if (unit.State == InventoryUnitState.OnHand)
            {
                QuantityReserved--;
            }
            unit.Cancel();
        }

        RaiseDomainEvent(new StockItemEvents.StockReleased(this, quantity, orderId.ToString()));
        return Result.Success;
    }

    /// <summary>
    /// Confirms that items have physically left the warehouse.
    /// </summary>
    public ErrorOr<Success> Fulfill(int quantity, Guid shipmentId, string reference, decimal unitCost = 0)
    {
        // Guard: Fulfillment requires a traceable audit reference
        if (string.IsNullOrWhiteSpace(reference))
            return StockItemErrors.ReferenceRequired(nameof(Fulfill));

        if (quantity <= 0) return StockItemErrors.InvalidQuantity(1, int.MaxValue);

        // Business Rule: Chain of Custody
        // We can only ship items that were previously reserved (OnHand)
        var unitsToShip = InventoryUnits
            .Where(u => u.State == InventoryUnitState.OnHand)
            .Take(quantity)
            .ToList();

        // Guard: Prevent shipping items that haven't been picked/reserved
        if (unitsToShip.Count < quantity)
            return StockItemErrors.MissingAllocations(quantity, unitsToShip.Count);

        var balanceBefore = QuantityOnHand;
        QuantityReserved -= unitsToShip.Count;
        QuantityOnHand -= quantity;

        foreach (var unit in unitsToShip)
        {
            unit.Ship(shipmentId);
        }

        // Business Rule: Finalize the transaction in the ledger
        var movement = StockMovement.Create(Id, -quantity, balanceBefore, StockMovementType.Sale, unitCost, StockItemConstraints.Movements.FulfillmentReason, reference);
        StockMovements.Add(movement);

        RaiseDomainEvent(new StockItemEvents.StockFilled(this, quantity, reference));
        return Result.Success;
    }

    /// <summary>
    /// Marks the item as inactive but preserves ledger history.
    /// </summary>
    public ErrorOr<Deleted> Delete()
    {
        if (IsDeleted) return Result.Deleted;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new StockItemEvents.StockItemDeleted(this));
        return Result.Deleted;
    }

    /// <summary>
    /// Re-activates a soft-deleted stock record.
    /// </summary>
    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted) return Result.Success;

        IsDeleted = false;
        DeletedAt = null;

        RaiseDomainEvent(new StockItemEvents.StockItemRestored(this));
        return Result.Success;
    }

    /// <summary>
    /// Configures the risk policy for selling items not physically in stock.
    /// </summary>
    public ErrorOr<Success> SetBackorderPolicy(bool backorderable, int limit)
    {
        // Guard: Limit must be a logical floor
        if (limit < 0) return StockItemErrors.InvalidQuantity(0, int.MaxValue);

        Backorderable = backorderable;
        BackorderLimit = limit;

        RaiseDomainEvent(new StockItemEvents.BackorderPolicyChanged(this, backorderable, limit));
        return Result.Success;
    }
}