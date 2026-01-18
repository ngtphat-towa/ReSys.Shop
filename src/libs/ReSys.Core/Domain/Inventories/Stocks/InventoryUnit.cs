using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Stocks;

/// <summary>
/// Represents a single unit of inventory allocated to an order line item.
/// Acts as a granular link between bulk stock and specific order fulfillment.
/// </summary>
public sealed class InventoryUnit : Aggregate, IAuditable, ISoftDeletable
{
    public Guid StockItemId { get; private set; }
    public Guid VariantId { get; private set; }
    public Guid? StockLocationId { get; private set; }
    public Guid? OrderId { get; private set; }
    public Guid? ShipmentId { get; private set; }
    public Guid? LineItemId { get; private set; }

    public InventoryUnitState State { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? LotNumber { get; private set; }

    // ISoftDeletable implementation
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation
    public StockItem StockItem { get; private set; } = null!;

    private InventoryUnit() { }

    /// <summary>
    /// Factory for creating a new tracking unit for a specific order line.
    /// </summary>
    public static InventoryUnit Create(
        Guid stockItemId,
        Guid variantId,
        Guid lineItemId,
        Guid? stockLocationId = null,
        InventoryUnitState initialState = InventoryUnitState.Pending)
    {
        var unit = new InventoryUnit
        {
            Id = Guid.NewGuid(),
            StockItemId = stockItemId,
            VariantId = variantId,
            LineItemId = lineItemId,
            StockLocationId = stockLocationId,
            State = initialState
        };

        unit.RaiseDomainEvent(new InventoryUnitEvents.InventoryUnitCreated(unit));
        return unit;
    }

    /// <summary>
    /// Allocates physical stock to this unit.
    /// </summary>
    public ErrorOr<Success> Reserve(Guid orderId)
    {
        // Guard: A unit can only move to OnHand from Pending or Backordered
        if (State != InventoryUnitState.Pending && State != InventoryUnitState.Backordered)
            return InventoryUnitErrors.InvalidStateTransition(State, nameof(Reserve));

        TransitionTo(InventoryUnitState.OnHand);
        OrderId = orderId;
        return Result.Success;
    }

    /// <summary>
    /// Marks the unit as promised but awaiting stock replenishment.
    /// </summary>
    public ErrorOr<Success> Backorder(Guid orderId)
    {
        // Guard: Only a fresh unit can be marked as backordered
        if (State != InventoryUnitState.Pending)
            return InventoryUnitErrors.InvalidStateTransition(State, nameof(Backorder));

        TransitionTo(InventoryUnitState.Backordered);
        OrderId = orderId;
        return Result.Success;
    }

    /// <summary>
    /// Confirms that the physical unit has left the warehouse.
    /// </summary>
    public ErrorOr<Success> Ship(Guid shipmentId)
    {
        // Guard: Only items physically in stock (Reserved) can be shipped
        if (State != InventoryUnitState.OnHand)
            return InventoryUnitErrors.InvalidStateTransition(State, nameof(Ship));

        TransitionTo(InventoryUnitState.Shipped);
        ShipmentId = shipmentId;
        return Result.Success;
    }

    /// <summary>
    /// Cancels the allocation, typically used during order cancellation.
    /// </summary>
    public ErrorOr<Success> Cancel()
    {
        // Guard: Cannot cancel what has already left the building
        if (State == InventoryUnitState.Shipped)
            return InventoryUnitErrors.AlreadyShipped;

        TransitionTo(InventoryUnitState.Canceled);
        return Result.Success;
    }

    /// <summary>
    /// Handles the return of a shipped unit.
    /// </summary>
    public ErrorOr<Success> Return()
    {
        // Guard: Can only return units that were actually shipped
        if (State != InventoryUnitState.Shipped)
            return InventoryUnitErrors.InvalidStateTransition(State, nameof(Return));

        TransitionTo(InventoryUnitState.Returned);
        return Result.Success;
    }

    /// <summary>
    /// Marks the unit as non-sellable due to damage.
    /// </summary>
    public void MarkAsDamaged() => TransitionTo(InventoryUnitState.Damaged);

    /// <summary>
    /// Logical removal of the unit while preserving history.
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
    /// Restores a logically deleted unit.
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
    /// Assigns a unique tracking identifier to this physical unit.
    /// </summary>
    public void SetSerialNumber(string serialNumber)
    {
        SerialNumber = serialNumber?.Trim();
    }
}
