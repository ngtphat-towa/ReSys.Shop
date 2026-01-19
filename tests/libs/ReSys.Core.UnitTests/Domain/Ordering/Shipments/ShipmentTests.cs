using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Core.Domain.Ordering.InventoryUnits;

namespace ReSys.Core.UnitTests.Domain.Ordering.Shipments;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Domain", "Shipment")]
public class ShipmentTests
{
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _locationId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should successfully initialize shipment")]
    public void Create_Should_InitializeShipment()
    {
        // Act
        var result = Shipment.Create(_orderId, _locationId, 500);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.OrderId.Should().Be(_orderId);
        result.Value.StockLocationId.Should().Be(_locationId);
        result.Value.CostCents.Should().Be(500);
        result.Value.State.Should().Be(Shipment.ShipmentState.Pending);
        result.Value.DomainEvents.Should().ContainSingle(e => e is ShipmentEvents.ShipmentCreated);
    }

    [Fact(DisplayName = "StateMachine: Should flow through Pick, Pack, Ship, Deliver")]
    public void StateMachine_Should_FlowThroughLifecycle()
    {
        // 1. Arrange
        var shipment = Shipment.Create(_orderId, _locationId).Value;

        // 2. Act & Assert: Pick
        shipment.MarkAsPicked().IsError.Should().BeFalse();
        shipment.State.Should().Be(Shipment.ShipmentState.Picked);

        // 3. Act & Assert: Pack
        shipment.MarkAsPacked().IsError.Should().BeFalse();
        shipment.State.Should().Be(Shipment.ShipmentState.Packed);

        // 4. Act & Assert: Ship
        shipment.Ship("TRACK-123").IsError.Should().BeFalse();
        shipment.State.Should().Be(Shipment.ShipmentState.Shipped);
        shipment.TrackingNumber.Should().Be("TRACK-123");

        // 5. Act & Assert: Deliver
        shipment.Deliver().IsError.Should().BeFalse();
        shipment.State.Should().Be(Shipment.ShipmentState.Delivered);
    }

    [Fact(DisplayName = "Ship: Should fail if tracking number is missing")]
    public void Ship_ShouldFail_IfTrackingMissing()
    {
        // Arrange
        var shipment = Shipment.Create(_orderId, _locationId).Value;
        shipment.MarkAsPicked();
        shipment.MarkAsPacked();

        // Act
        var result = shipment.Ship("");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Shipment.TrackingNumberRequired");
    }

    [Fact(DisplayName = "Cancel: Should transition to Canceled state")]
    public void Cancel_Should_WorkIfPending()
    {
        // Arrange
        var shipment = Shipment.Create(_orderId, _locationId).Value;

        // Act
        var result = shipment.Cancel();

        // Assert
        result.IsError.Should().BeFalse();
        shipment.State.Should().Be(Shipment.ShipmentState.Canceled);
    }

    [Fact(DisplayName = "Cancel: Should fail if already shipped")]
    public void Cancel_ShouldFail_IfShipped()
    {
        // Arrange
        var shipment = Shipment.Create(_orderId, _locationId).Value;
        shipment.MarkAsPicked();
        shipment.MarkAsPacked();
        shipment.Ship("T1");

        // Act
        var result = shipment.Cancel();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ShipmentErrors.AlreadyShipped);
    }
}