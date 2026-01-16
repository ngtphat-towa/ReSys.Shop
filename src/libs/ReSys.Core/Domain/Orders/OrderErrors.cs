using ErrorOr;

namespace ReSys.Core.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Order.NotFound",
        description: $"Order with ID '{id}' was not found.");

    public static Error InvalidStateTransition(string from, string to) => Error.Validation(
        code: "Order.InvalidStateTransition",
        description: $"Cannot transition from {from} to {to}.");

    public static Error EmptyCart => Error.Validation(
        code: "Order.EmptyCart",
        description: "Cannot checkout an empty cart.");
}
