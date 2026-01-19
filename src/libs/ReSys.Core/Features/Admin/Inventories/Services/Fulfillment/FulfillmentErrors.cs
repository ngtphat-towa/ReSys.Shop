using ErrorOr;

namespace ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

public static class FulfillmentErrors
{
    public static Error InsufficientTotalStock(string sku) => Error.Validation(
        code: "Fulfillment.InsufficientTotalStock",
        description: $"Even across all warehouses, there is not enough stock to fulfill SKU: {sku}.");

    public static Error NoFulfillableLocations => Error.Failure(
        code: "Fulfillment.NoFulfillableLocations",
        description: "No active or fulfillable warehouses were found in the system.");

    public static Error EmptyOrder => Error.Validation(
        code: "Fulfillment.EmptyOrder",
        description: "Cannot plan fulfillment for an empty order.");
}
