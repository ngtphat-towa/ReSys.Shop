using ErrorOr;

namespace ReSys.Core.Domain.Ordering.InventoryUnits;

public static class InventoryUnitErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "InventoryUnit.NotFound",
        description: $"Inventory unit with ID '{id}' was not found.");

    public static Error InvalidStateTransition(InventoryUnitState current, string action) => Error.Conflict(
        code: "InventoryUnit.InvalidStateTransition",
        description: $"Cannot perform '{action}' on a unit in state '{current}'.");

    public static Error AlreadyShipped => Error.Conflict(
        code: "InventoryUnit.AlreadyShipped",
        description: "This inventory unit has already been shipped.");

    public static Error AlreadyCanceled => Error.Conflict(
        code: "InventoryUnit.AlreadyCanceled",
        description: "This inventory unit has been canceled.");
}
