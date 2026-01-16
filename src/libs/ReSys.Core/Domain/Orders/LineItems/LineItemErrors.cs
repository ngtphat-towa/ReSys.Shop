using ErrorOr;

namespace ReSys.Core.Domain.Orders.LineItems;

public static class LineItemErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "LineItem.NotFound",
        description: $"Line item with ID '{id}' was not found.");

    public static Error InvalidQuantity => Error.Validation(
        code: "LineItem.InvalidQuantity",
        description: $"Quantity must be at least {LineItemConstraints.MinQuantity}.");
}
