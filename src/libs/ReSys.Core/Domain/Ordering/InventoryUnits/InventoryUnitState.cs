namespace ReSys.Core.Domain.Ordering.InventoryUnits;

/// <summary>
/// Represents the distinct lifecycle stages of an inventory unit.
/// </summary>
public enum InventoryUnitState
{
    Pending,     // Initial state, not yet allocated
    OnHand,      // Reserved from physical stock
    Backordered, // Allocated to an order but not physically available
    Shipped,     // Physically dispatched
    Returned,    // Returned by customer
    Damaged,     // Found damaged during picking
    Canceled     // Allocation canceled before shipping
}
