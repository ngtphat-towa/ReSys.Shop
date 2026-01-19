using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Stocks;

public static class StockItemErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "StockItem.NotFound",
        description: $"Stock item with ID '{id}' was not found.");

    public static Error SkuRequired => Error.Validation(
        code: "StockItem.SkuRequired",
        description: "SKU is required.");

    public static Error InvalidQuantity(int min, int max) => Error.Validation(
        code: "StockItem.InvalidQuantity",
        description: $"Quantity must be between {min} and {max}.");

    public static Error ZeroQuantityMovement => Error.Validation(
        code: "StockItem.ZeroQuantityMovement",
        description: "Cannot record a stock movement with zero quantity.");

    public static Error InsufficientStock(int available, int requested) => Error.Validation(
        code: "StockItem.InsufficientStock",
        description: $"Insufficient stock. Available: {available}, Requested: {requested}.");

    public static Error BackorderLimitExceeded(int limit, int requested) => Error.Conflict(
        code: "StockItem.BackorderLimitExceeded",
        description: $"Operation would exceed the backorder limit of {limit}. Current available: {requested}.");

    public static Error InvalidRelease(int reserved, int requested) => Error.Validation(
        code: "StockItem.InvalidRelease",
        description: $"Cannot release {requested} units. Only {reserved} units are currently reserved.");

    public static Error ReservedExceedsOnHand(int onHand, int reserved) => Error.Conflict(
        code: "StockItem.ReservedExceedsOnHand",
        description: $"Reservation would exceed physical stock. On Hand: {onHand}, Requested Reservation: {reserved}.");

    public static Error ReferenceRequired(string action) => Error.Validation(
        code: "StockItem.ReferenceRequired",
        description: $"A reference (e.g., Order ID) is required for '{action}' operations.");

    public static Error MissingAllocations(int required, int actual) => Error.Conflict(
        code: "StockItem.MissingAllocations",
        description: $"Cannot fulfill {required} units. Only {actual} are reserved OnHand.");
}
