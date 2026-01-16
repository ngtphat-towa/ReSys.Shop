using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Orders.Shipments;

public sealed class Shipment : Entity
{
    public Guid OrderId { get; private set; }
    public Guid StockLocationId { get; private set; }
    public ShipmentState State { get; private set; } = ShipmentState.Pending;
    public string? TrackingNumber { get; private set; }
    public decimal Cost { get; private set; }

    private Shipment() { }

    public static Shipment Create(Guid orderId, Guid stockLocationId, decimal cost = 0)
    {
        return new Shipment
        {
            OrderId = orderId,
            StockLocationId = stockLocationId,
            Cost = cost,
            State = ShipmentState.Pending
        };
    }

    public enum ShipmentState
    {
        Pending,
        Ready,
        Shipped,
        Canceled
    }
}
