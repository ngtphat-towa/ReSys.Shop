using ReSys.Core.Domain.Ordering.InventoryUnits;

namespace ReSys.Core.UnitTests.Domain.Ordering.InventoryUnits;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Domain", "InventoryUnit")]
public class InventoryUnitTests
{
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _lineItemId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _shipmentId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should successfully initialize unit")]
    public void Create_Should_InitializeUnit()
    {
        // Act
        var unit = InventoryUnit.Create(_variantId, _lineItemId);

        // Assert
        unit.VariantId.Should().Be(_variantId);
        unit.LineItemId.Should().Be(_lineItemId);
        unit.State.Should().Be(InventoryUnitState.Pending);
        unit.Pending.Should().BeTrue();
        unit.DomainEvents.Should().ContainSingle(e => e is InventoryUnitEvents.InventoryUnitCreated);
    }

    [Fact(DisplayName = "Reserve: Should transition to OnHand and set OrderId")]
    public void Reserve_Should_TransitionToOnHand()
    {
        // Arrange
        var unit = InventoryUnit.Create(_variantId, _lineItemId);

        // Act
        var result = unit.Reserve(_orderId);

        // Assert
        result.IsError.Should().BeFalse();
        unit.State.Should().Be(InventoryUnitState.OnHand);
        unit.OrderId.Should().Be(_orderId);
        unit.DomainEvents.Should().Contain(e => e is InventoryUnitEvents.InventoryUnitStateChanged);
    }

    [Fact(DisplayName = "Ship: Should transition to Shipped and clear Pending flag")]
    public void Ship_Should_TransitionToShipped()
    {
        // Arrange
        var unit = InventoryUnit.Create(_variantId, _lineItemId);
        unit.Reserve(_orderId);

        // Act
        var result = unit.Ship(_shipmentId);

        // Assert
        result.IsError.Should().BeFalse();
        unit.State.Should().Be(InventoryUnitState.Shipped);
        unit.ShipmentId.Should().Be(_shipmentId);
        unit.Pending.Should().BeFalse();
    }

    [Fact(DisplayName = "Ship: Should fail if not in OnHand state")]
    public void Ship_ShouldFail_IfNotOnHand()
    {
        // Arrange
        var unit = InventoryUnit.Create(_variantId, _lineItemId); // State is Pending

        // Act
        var result = unit.Ship(_shipmentId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("InventoryUnit.InvalidStateTransition");
    }

    [Fact(DisplayName = "Cancel: Should transition to Canceled and clear Pending flag")]
    public void Cancel_Should_TransitionToCanceled()
    {
        // Arrange
        var unit = InventoryUnit.Create(_variantId, _lineItemId);

        // Act
        var result = unit.Cancel();

        // Assert
        result.IsError.Should().BeFalse();
        unit.State.Should().Be(InventoryUnitState.Canceled);
        unit.Pending.Should().BeFalse();
    }

    [Fact(DisplayName = "Cancel: Should fail if already shipped")]
    public void Cancel_ShouldFail_IfShipped()
    {
        // Arrange
        var unit = InventoryUnit.Create(_variantId, _lineItemId);
        unit.Reserve(_orderId);
        unit.Ship(_shipmentId);

        // Act
        var result = unit.Cancel();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(InventoryUnitErrors.AlreadyShipped);
    }

    [Fact(DisplayName = "Return: Should transition to Returned from Shipped state")]
    public void Return_Should_WorkIfShipped()
    {
        // Arrange
        var unit = InventoryUnit.Create(_variantId, _lineItemId);
        unit.Reserve(_orderId);
        unit.Ship(_shipmentId);

        // Act
        var result = unit.Return();

        // Assert
        result.IsError.Should().BeFalse();
        unit.State.Should().Be(InventoryUnitState.Returned);
    }

    [Fact(DisplayName = "MarkAsDamaged: Should transition to Damaged state")]
    public void MarkAsDamaged_Should_ChangeState()
    {
        // Arrange
        var unit = InventoryUnit.Create(_variantId, _lineItemId);

        // Act
        var result = unit.MarkAsDamaged();

        // Assert
        result.IsError.Should().BeFalse();
        unit.State.Should().Be(InventoryUnitState.Damaged);
    }

    [Fact(DisplayName = "Delete: Should set soft delete state")]
    public void Delete_Should_SetDeleted()
    {
        // Arrange
        var unit = InventoryUnit.Create(_variantId, _lineItemId);

        // Act
        var result = unit.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        unit.IsDeleted.Should().BeTrue();
        unit.DeletedAt.Should().NotBeNull();
    }
}