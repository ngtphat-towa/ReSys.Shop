using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Movements;

public static class StockTransferErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "StockTransfer.NotFound",
        description: $"Stock transfer with ID '{id}' was not found.");

    public static Error SameLocation => Error.Validation(
        code: "StockTransfer.SameLocation",
        description: "Source and destination locations cannot be the same.");

    public static Error InvalidStatusTransition(StockTransferStatus current, string action) => Error.Conflict(
        code: "StockTransfer.InvalidStatusTransition",
        description: $"Cannot perform '{action}' while transfer is in '{current}' status.");

    public static Error EmptyTransfer => Error.Validation(
        code: "StockTransfer.EmptyTransfer",
        description: "Cannot ship a transfer with no items.");

    public static Error ItemNotFound(Guid variantId) => Error.NotFound(
        code: "StockTransfer.ItemNotFound",
        description: $"Item with variant ID '{variantId}' is not part of this transfer.");
}
