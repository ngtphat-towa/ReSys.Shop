using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Ordering.InventoryUnits;

using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Shipments;

/// <summary>
/// Represents a physical package moving from a specific warehouse to a customer.
/// Orchestrates the warehouse lifecycle (Pick -> Pack -> Ship).
/// It manages the transition of physical inventory units from warehouse shelves to carrier hands.
/// </summary>
public sealed class Shipment : Aggregate
{
    /// <summary>
    /// Sequential stages of physical fulfillment.
    /// </summary>
    public enum ShipmentState
    {
        /// <summary>Initialized but not started.</summary>
        Pending,
        /// <summary>Allocated and ready for warehouse tasks.</summary>
        Ready,      
        /// <summary>Items removed from shelf.</summary>
        Picked,     
        /// <summary>Items in box.</summary>
        Packed,     
        /// <summary>Handed to carrier.</summary>
        Shipped,    
        /// <summary>Received by customer.</summary>
        Delivered,  
        /// <summary>Abort fulfillment.</summary>
        Canceled
    }

    #region Properties
    /// <summary>Source order reference.</summary>
    public Guid OrderId { get; set; }

    /// <summary>The physical warehouse location where items are picked.</summary>
    public Guid StockLocationId { get; set; }

    /// <summary>Human-readable shipment number (e.g. SHP-20260121-1234).</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Current logistical status.</summary>
    public ShipmentState State { get; set; } = ShipmentState.Pending;

    /// <summary>Carrier tracking identifier.</summary>
    public string? TrackingNumber { get; set; }

    /// <summary>Minor units (cents) for specific shipment costs.</summary>
    public long CostCents { get; set; }

    /// <summary>Timestamp of picking.</summary>
    public DateTimeOffset? PickedAt { get; set; }

    /// <summary>Timestamp of packing.</summary>
    public DateTimeOffset? PackedAt { get; set; }

    /// <summary>Timestamp of handover to carrier.</summary>
    public DateTimeOffset? ShippedAt { get; set; }

    /// <summary>Timestamp of delivery confirmation.</summary>
    public DateTimeOffset? DeliveredAt { get; set; }

    // Relationships
    /// <summary>Parent order navigation.</summary>
    public Order Order { get; set; } = null!;

    /// <summary>Granular link to physical items contained in this package.</summary>
    public ICollection<InventoryUnit> InventoryUnits { get; set; } = new List<InventoryUnit>();
    #endregion

    public Shipment() { }

    #region Factory Methods
    /// <summary>
    /// Factory for creating a new warehouse shipment.
    /// </summary>
    public static ErrorOr<Shipment> Create(Guid orderId, Guid stockLocationId, long costCents = 0)
    {
        if (orderId == Guid.Empty) return Error.Validation("Shipment.OrderIdRequired", "OrderId is required.");
        if (stockLocationId == Guid.Empty) return Error.Validation("Shipment.LocationRequired", "StockLocationId is required.");

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            StockLocationId = stockLocationId,
            Number = $"{ShipmentConstraints.NumberPrefix}{DateTimeOffset.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}",
            CostCents = costCents,
            State = ShipmentState.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        shipment.RaiseDomainEvent(new ShipmentEvents.ShipmentCreated(shipment));
        return shipment;
    }
    #endregion

    #region State Transitions
    /// <summary>
    /// Signals the warehouse team to begin picking items.
    /// </summary>
    public ErrorOr<Success> MarkAsPicked()
    {
        // Guard: Prevent out-of-order execution
        if (State != ShipmentState.Ready && State != ShipmentState.Pending)
            return ShipmentErrors.InvalidStateTransition(State, ShipmentState.Picked);

        State = ShipmentState.Picked;
        PickedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new ShipmentEvents.ShipmentPicked(this));
        return Result.Success;
    }

    /// <summary>
    /// Signals that items are now boxed and taped.
    /// </summary>
    public ErrorOr<Success> MarkAsPacked()
    {
        if (State != ShipmentState.Picked)
            return ShipmentErrors.InvalidStateTransition(State, ShipmentState.Packed);

        State = ShipmentState.Packed;
        PackedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new ShipmentEvents.ShipmentPacked(this));
        return Result.Success;
    }

    /// <summary>
    /// Finalizes the handoff to the carrier. Triggers 'Ship' on all contained units.
    /// </summary>
    public ErrorOr<Success> Ship(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber)) return ShipmentErrors.TrackingNumberRequired;
        if (State != ShipmentState.Packed && State != ShipmentState.Picked)
            return ShipmentErrors.InvalidStateTransition(State, ShipmentState.Shipped);

        State = ShipmentState.Shipped;
        ShippedAt = DateTimeOffset.UtcNow;
        TrackingNumber = trackingNumber.Trim();

        // Chain of Custody: Update the granular units
        foreach (var unit in InventoryUnits)
        {
            unit.Ship(Id);
        }

        RaiseDomainEvent(new ShipmentEvents.ShipmentShipped(this));
        return Result.Success;
    }

    /// <summary>
    /// Transitions the shipment to its final terminal state.
    /// </summary>
    public ErrorOr<Success> Deliver()
    {
        if (State != ShipmentState.Shipped)
            return ShipmentErrors.InvalidStateTransition(State, ShipmentState.Delivered);

        State = ShipmentState.Delivered;
        DeliveredAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new ShipmentEvents.ShipmentDelivered(this));
        return Result.Success;
    }

    /// <summary>
    /// Aborts the shipment. Fails if items have already left the building.
    /// </summary>
    public ErrorOr<Success> Cancel()
    {
        if (State == ShipmentState.Shipped || State == ShipmentState.Delivered)
            return ShipmentErrors.AlreadyShipped;

        State = ShipmentState.Canceled;

        foreach (var unit in InventoryUnits)
        {
            unit.Cancel();
        }

        RaiseDomainEvent(new ShipmentEvents.ShipmentCanceled(this));
        return Result.Success;
    }

    /// <summary>
    /// Marks the shipment as eligible for warehouse activities.
    /// </summary>
    public void SetReady() => State = ShipmentState.Ready;
    #endregion
}
