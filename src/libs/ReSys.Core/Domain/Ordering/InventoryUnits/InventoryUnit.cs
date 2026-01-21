using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Ordering.Shipments;

using ErrorOr;

namespace ReSys.Core.Domain.Ordering.InventoryUnits;

/// <summary>
/// Represents a single unit of inventory allocated to fulfill part of an order line item.
/// Each record represents exactly ONE physical item (Granular Tracking).
/// Acts as the source of truth for physical earmarking and warehouse pick-lists.
/// </summary>
public sealed class InventoryUnit : Aggregate, ISoftDeletable
{
    #region Properties
    public Guid? StockItemId { get; set; }
    public Guid VariantId { get; set; }
    public Guid? StockLocationId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? ShipmentId { get; set; }
    public Guid? LineItemId { get; set; }

    public InventoryUnitState State { get; set; } = InventoryUnitState.Pending;
    public string? SerialNumber { get; set; }
    public string? LotNumber { get; set; }

    /// <summary>
    /// Control Flag: If true, inventory is reserved in DB but not yet physically decremented.
    /// Transitions to false when order is finalized or items physically leave the building.
    /// </summary>
    public bool Pending { get; set; } = true;

    // ISoftDeletable implementation
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation
    public StockItem? StockItem { get; set; }
    public Shipment? Shipment { get; set; }
    #endregion

    public InventoryUnit() { }

    #region Factory Methods
    /// <summary>
    /// Factory for creating a new tracking unit for a specific order line.
    /// Business Rule: Create one unit per physical item requested.
    /// </summary>
    public static InventoryUnit Create(
        Guid variantId,
        Guid lineItemId,
        Guid? stockItemId = null,
        Guid? stockLocationId = null,
        InventoryUnitState initialState = InventoryUnitState.Pending,
        bool pending = true)
    {
        var unit = new InventoryUnit
        {
            Id = Guid.NewGuid(),
            StockItemId = stockItemId,
            VariantId = variantId,
            LineItemId = lineItemId,
            StockLocationId = stockLocationId,
            State = initialState,
            Pending = pending
        };

        unit.RaiseDomainEvent(new InventoryUnitEvents.InventoryUnitCreated(unit));
        return unit;
    }
    #endregion

    /// <summary>
    /// Assigns the physical stock record to this unit.
    /// </summary>
    public void SetStockItem(Guid stockItemId, Guid stockLocationId)
    {
        StockItemId = stockItemId;
        StockLocationId = stockLocationId;
    }

        #region State Transitions
        /// <summary>
        /// Allocates physical stock to this unit from a specific location.
        /// This represents a 'Soft Lock' on the inventory item.
        /// </summary>
        public ErrorOr<Success> Reserve(Guid orderId)
        {
            // Guard: Prevent double-reservation or reserving backorders.
            if (State != InventoryUnitState.Pending && State != InventoryUnitState.Backordered)
                return InventoryUnitErrors.InvalidStateTransition(State, nameof(Reserve));
    
            TransitionTo(InventoryUnitState.OnHand);
                    OrderId = orderId;
                    return Result.Success;
                }
            
                /// <summary>
                /// Links this unit to a specific warehouse shipment.
                /// This prepares the unit for the Pick/Pack workflow.
                /// </summary>
                public ErrorOr<Success> AssignToShipment(Guid shipmentId)
                {
                    // Guard: Cannot re-assign items that have already left or been canceled
                    if (State == InventoryUnitState.Shipped || State == InventoryUnitState.Canceled)
                        return InventoryUnitErrors.InvalidStateTransition(State, nameof(AssignToShipment));
            
                    ShipmentId = shipmentId;
                    return Result.Success;
                }
            
                /// <summary>
                /// Marks the unit as promised to an order but awaiting stock replenishment.        /// This represents a "Physical Debt" in the ledger.
        /// It ensures that as soon as stock arrives, this order has priority.
        /// </summary>
        public ErrorOr<Success> Backorder(Guid orderId)
        {
            // Guard: Only a fresh unit or a canceled allocation can be backordered.
            if (State != InventoryUnitState.Pending && State != InventoryUnitState.Canceled)
                return InventoryUnitErrors.InvalidStateTransition(State, nameof(Backorder));
    
            TransitionTo(InventoryUnitState.Backordered);
            OrderId = orderId;
            return Result.Success;
        }
    
        /// <summary>
        /// Confirms the unit has left the warehouse. Auto-finalizes the ledger state.
        /// This is the final physical step in fulfillment.
        /// </summary>
        public ErrorOr<Success> Ship(Guid shipmentId)
        {
            // Guard: Can only ship items currently on hand (picked).
            if (State != InventoryUnitState.OnHand)
                return InventoryUnitErrors.InvalidStateTransition(State, nameof(Ship));
    
            TransitionTo(InventoryUnitState.Shipped);
            ShipmentId = shipmentId;
            
            // Business Rule: Once shipped, the item is no longer 'Pending' in the ledger.
            Pending = false; 
            return Result.Success;
        }
    
        /// <summary>
        /// Cancels the allocation. Auto-finalizes to release the reservation.
        /// This returns the item to 'Available' pool in the warehouse.
        /// </summary>
        public ErrorOr<Success> Cancel()
        {
            // Guard: Once items leave the building, they cannot be canceled (must be returned).
            if (State == InventoryUnitState.Shipped)
                return InventoryUnitErrors.AlreadyShipped;
    
            TransitionTo(InventoryUnitState.Canceled);
            Pending = false; 
            return Result.Success;
        }
    /// <summary>
    /// Marks the unit as returned by the customer.
    /// </summary>
    public ErrorOr<Success> Return()
    {
        // Guard: Only shipped items can be returned
        if (State != InventoryUnitState.Shipped)
            return InventoryUnitErrors.InvalidStateTransition(State, nameof(Return));

        TransitionTo(InventoryUnitState.Returned);
        return Result.Success;
    }

    /// <summary>
    /// Marks the unit as non-sellable due to physical damage.
    /// </summary>
    public ErrorOr<Success> MarkAsDamaged()
    {
        // Guard: Shipped items cannot be marked as damaged until returned
        if (State == InventoryUnitState.Shipped)
            return InventoryUnitErrors.InvalidStateTransition(State, nameof(MarkAsDamaged));

        TransitionTo(InventoryUnitState.Damaged);
        return Result.Success;
    }

    /// <summary>
    /// Commits the inventory decrement. Typically called when an order reaches the 'Complete' state.
    /// </summary>
    public void FinalizeUnit()
    {
        if (!Pending) return;
        Pending = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    #endregion

    #region Domain Methods
    /// <summary>
    /// Soft deletes the record while preserving audit history.
    /// </summary>
    public ErrorOr<Deleted> Delete()
    {
        if (IsDeleted) return Result.Deleted;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new InventoryUnitEvents.InventoryUnitDeleted(this));
        return Result.Deleted;
    }

    /// <summary>
    /// Re-activates a soft-deleted unit.
    /// </summary>
    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted) return Result.Success;

        IsDeleted = false;
        DeletedAt = null;

        RaiseDomainEvent(new InventoryUnitEvents.InventoryUnitRestored(this));
        return Result.Success;
    }

    private void TransitionTo(InventoryUnitState newState)
    {
        if (State == newState) return;
        var oldState = State;
        State = newState;
        RaiseDomainEvent(new InventoryUnitEvents.InventoryUnitStateChanged(this, oldState, newState));
    }

    /// <summary>
    /// Assigns a unique tracking identifier (e.g., scanned during picking).
    /// </summary>
    public void SetSerialNumber(string? serialNumber)
    {
        if (serialNumber?.Length > InventoryUnitConstraints.SerialNumberMaxLength) return;
        SerialNumber = serialNumber?.Trim();
    }

    /// <summary>
    /// Assigns a lot/batch identifier for perishable or tracked goods.
    /// </summary>
    public void SetLotNumber(string? lotNumber)
    {
        if (lotNumber?.Length > InventoryUnitConstraints.LotNumberMaxLength) return;
        LotNumber = lotNumber?.Trim();
    }
    #endregion
}
