using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Ordering.InventoryUnits;

namespace ReSys.Core.Domain.Inventories.Stocks;

/// <summary>
/// Represents the physical and logical inventory of a product variant at a specific location.
/// Orchestrates the balance between physical stock, customer reservations, and the audit ledger.
/// </summary>
public sealed class StockItem : Aggregate, IHasMetadata, ISoftDeletable
{
    public Guid VariantId { get; set; }
    public Guid StockLocationId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    
    /// <summary>
    /// Logical counter representing the total "promised" stock.
    /// This includes items earmarked on the shelf (OnHand) AND items promised but not yet in stock (Backordered).
    /// </summary>
    public int QuantityReserved { get; set; }
    
    public bool Backorderable { get; set; } = true;
    public int BackorderLimit { get; set; } = StockItemConstraints.DefaultMaxBackorderQuantity;

    // ISoftDeletable implementation
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation
    public Variant Variant { get; set; } = null!;
    public StockLocation StockLocation { get; set; } = null!;

    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<InventoryUnit> InventoryUnits { get; set; } = new List<InventoryUnit>();

    /// <summary>
    /// Logic: Real-time availability calculation. 
    /// If backorderable, we show the true potential (can be negative). 
    /// If not, we never show less than zero to the commercial layer.
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
        // Note: We check against (newQuantity - QuantityReserved) to ensure existing earmarks are respected.
        if (!Backorderable && (newQuantity - QuantityReserved) < 0)
            return StockItemErrors.InsufficientStock(CountAvailable, Math.Abs(quantity));

        QuantityOnHand = newQuantity;

        // Business Rule: Backorder Promotion (FIFO)
        // If we added physical stock, we must prioritize existing customer debt (Backorders)
        if (quantity > 0)
        {
            var backorderedCount = InventoryUnits.Count(u => u.State == InventoryUnitState.Backordered);
            var promotableCount = Math.Min(quantity, backorderedCount);

            var backorderedUnits = InventoryUnits
                .Where(u => u.State == InventoryUnitState.Backordered)
                .OrderBy(u => u.CreatedAt) // First-come, first-served
                .Take(promotableCount)
                .ToList();

            foreach (var unit in backorderedUnits)
            {
                // Transition: Promote from Backordered to OnHand (physical earmark)
                // Note: We don't increment QuantityReserved here because the "Promise" 
                // was already counted in the total reserved count when the unit was created.
                unit.Reserve(unit.OrderId!.Value);
            }
        }
        
        // Business Rule: Record the physical truth in the Ledger
        var movement = StockMovement.Create(Id, quantity, balanceBefore, type, unitCost, reason, reference);
        StockMovements.Add(movement);

        RaiseDomainEvent(new StockItemEvents.StockAdjusted(this, quantity, type, reference));
        return Result.Success;
    }

    /// <summary>
    /// Promises stock to an order. Handles both physical earmarks and backorder debt.
    /// </summary>
    public ErrorOr<Success> Reserve(int quantity, Guid orderId, Guid lineItemId)
    {
        // Guard: Prevent empty or negative reservations
        if (quantity <= 0) return StockItemErrors.InvalidQuantity(1, int.MaxValue);
        
        // Guard: Ensure availability if backordering is disabled
        if (!Backorderable && (QuantityOnHand - QuantityReserved) < quantity) 
            return StockItemErrors.InsufficientStock(CountAvailable, quantity);

        for (int i = 0; i < quantity; i++)
        {
            var unit = InventoryUnit.Create(Id, VariantId, lineItemId, StockLocationId, InventoryUnitState.Pending);
            
            // Logic: Determine if this unit can be physically earmarked (OnHand) or remains a promise (Backordered)
            if ((QuantityOnHand - QuantityReserved) > 0)
            {
                unit.Reserve(orderId);
            }
            else
            {
                unit.Backorder(orderId);
            }
            
            // Business Rule: QuantityReserved represents the total "Debt" (Sold but not Shipped).
            QuantityReserved++;
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
            // Business Rule: Decrement the total promise
            QuantityReserved--;
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
        // We prioritize shipping items that were previously reserved (OnHand).
        var unitsToShip = InventoryUnits
            .Where(u => u.State == InventoryUnitState.OnHand)
            .Take(quantity)
            .ToList();

        // Guard: If fulfilling more than reserved, ensure physical stock is available if backordering is off
        var extraRequired = Math.Max(0, quantity - unitsToShip.Count);
        var physicalAvailable = QuantityOnHand - QuantityReserved;
        
        if (!Backorderable && physicalAvailable < extraRequired)
            return StockItemErrors.InsufficientStock(CountAvailable, extraRequired);

        var balanceBefore = QuantityOnHand;
        
        // Finalize: Deduct from total promises and physical shelves
        QuantityReserved -= unitsToShip.Count;
        QuantityOnHand -= quantity;

        foreach (var unit in unitsToShip)
        {
            unit.Ship(shipmentId);
        }

        // Business Rule: If fulfilling more than reserved (over-fulfillment or direct sale)
        // We create and ship new units on-the-fly to ensure the audit trail remains complete.
        if (quantity > unitsToShip.Count)
        {
            var extra = quantity - unitsToShip.Count;
            for (int i = 0; i < extra; i++)
            {
                var unit = InventoryUnit.Create(Id, VariantId, Guid.Empty, StockLocationId, InventoryUnitState.OnHand);
                unit.Reserve(Guid.Empty); // Link to a 'direct sale' or anonymous order if needed
                unit.Ship(shipmentId);
                InventoryUnits.Add(unit);
            }
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

    /// <summary>
    /// Assigns specific metadata to this stock record.
    /// </summary>
    public void SetMetadata(IDictionary<string, object?>? publicMetadata, IDictionary<string, object?>? privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
    }
}
