using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Shipments;

public static class ShipmentErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Shipment.NotFound",
        description: $"Shipment with ID '{id}' was not found.");

    public static Error Required => Error.Validation(
        code: "Shipment.Required",
        description: "Shipment is required.");

    public static Error InvalidStateTransition(Shipment.ShipmentState from, Shipment.ShipmentState to) => Error.Conflict(
        code: "Shipment.InvalidStateTransition",
        description: $"Cannot transition shipment from '{from}' to '{to}'.");

    public static Error AlreadyShipped => Error.Conflict(
        code: "Shipment.AlreadyShipped",
        description: "Cannot modify or cancel a shipment that has already been shipped.");

    public static Error TrackingNumberRequired => Error.Validation(
        code: "Shipment.TrackingNumberRequired",
        description: "A tracking number is required to mark a shipment as shipped.");

    public static Error NumberTooLong => Error.Validation(
        code: "Shipment.NumberTooLong",
        description: $"Shipment number cannot exceed {ShipmentConstraints.NumberMaxLength} characters.");
}