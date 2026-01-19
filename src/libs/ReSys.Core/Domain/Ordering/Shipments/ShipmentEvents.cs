using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Ordering.Shipments;

public static class ShipmentEvents
{
    public record ShipmentCreated(Shipment Shipment) : IDomainEvent;
    public record ShipmentStateChanged(Shipment Shipment, Shipment.ShipmentState OldState, Shipment.ShipmentState NewState) : IDomainEvent;
    public record ShipmentPicked(Shipment Shipment) : IDomainEvent;
    public record ShipmentPacked(Shipment Shipment) : IDomainEvent;
    public record ShipmentShipped(Shipment Shipment) : IDomainEvent;
    public record ShipmentDelivered(Shipment Shipment) : IDomainEvent;
    public record ShipmentCanceled(Shipment Shipment) : IDomainEvent;
}
