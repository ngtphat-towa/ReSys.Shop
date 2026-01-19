using ErrorOr;

namespace ReSys.Core.Domain.Ordering;

/// <summary>
/// Centralized business failure definitions for the Ordering process.
/// </summary>
public static class OrderErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Order.NotFound",
        description: $"Order with ID '{id}' was not found.");

    public static Error StoreRequired => Error.Validation(
        code: "Order.StoreRequired",
        description: "An order must be associated with a Store context.");

    public static Error EmptyCart => Error.Validation(
        code: "Order.EmptyCart",
        description: "Cannot proceed to checkout with an empty cart.");

    public static Error InvalidStateTransition(string from, string to) => Error.Conflict(
        code: "Order.InvalidStateTransition",
        description: $"Cannot transition order from '{from}' to '{to}'.");

    public static Error VariantRequired => Error.Validation(
        code: "Order.VariantRequired",
        description: "A valid product variant is required to add an item.");

    public static Error InsufficientPayment(decimal total, decimal received) => Error.Validation(
        code: "Order.InsufficientPayment",
        description: $"Authorized payment required: {total:C}. Received: {received:C}");

    public static Error CannotModifyInTerminalState => Error.Validation(
        code: "Order.CannotModifyInTerminalState",
        description: "Cannot modify an order that is already Completed or Canceled.");

    public static Error CannotCancelCompleted => Error.Conflict(
        code: "Order.CannotCancelCompleted",
        description: "Completed orders cannot be canceled.");

    public static Error AddressMissing => Error.Validation(
        code: "Order.AddressMissing", 
        description: "Shipping and Billing addresses are required.");

    public static Error ShippingMethodMissing => Error.Validation(
        code: "Order.ShippingMethodMissing", 
        description: "Shipping method must be selected.");
        
    public static Error IncompleteInventoryAllocation => Error.Conflict(
        code: "Order.IncompleteInventoryAllocation",
        description: "Some items are not fully allocated to a warehouse.");
}
