using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Stocks;

public static class StockItemErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "StockItem.NotFound",
        description: $"Stock item with ID '{id}' was not found.");

    public static Error InvalidQuantity => Error.Validation(
        code: "StockItem.InvalidQuantity",
        description: "Quantity must be non-negative.");

    public static Error InsufficientStock => Error.Validation(
        code: "StockItem.InsufficientStock",
        description: "Insufficient stock for operation.");

    public static Error InvalidRelease => Error.Validation(
        code: "StockItem.InvalidRelease",
        description: "Cannot release more than reserved.");
}
