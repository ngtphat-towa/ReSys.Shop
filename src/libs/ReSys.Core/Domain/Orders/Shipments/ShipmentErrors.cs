using ErrorOr;

namespace ReSys.Core.Domain.Orders.Shipments;

public static class ShipmentErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Shipment.NotFound",
        description: $"Shipment with ID '{id}' was not found.");

    public static Error Required => Error.Validation(
        code: "Shipment.Required",
        description: "Shipment is required.");
}
