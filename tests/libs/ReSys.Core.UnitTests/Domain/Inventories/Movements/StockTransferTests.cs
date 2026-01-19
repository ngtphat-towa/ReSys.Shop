using ReSys.Core.Domain.Inventories.Movements;

using FluentAssertions;

using Xunit;

namespace ReSys.Core.UnitTests.Domain.Inventories.Movements;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Domain", "StockTransfer")]
public class StockTransferTests
{
    [Fact(DisplayName = "Create should succeed with different locations")]
    public void Create_ShouldSucceed_WithDifferentLocations()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var destId = Guid.NewGuid();

        // Act
        var result = StockTransfer.Create(sourceId, destId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.SourceLocationId.Should().Be(sourceId);
        result.Value.DestinationLocationId.Should().Be(destId);
        result.Value.Status.Should().Be(StockTransferStatus.Draft);
    }

    [Fact(DisplayName = "Create should fail if locations are identical")]
    public void Create_ShouldFail_WithSameLocation()
    {
        var locationId = Guid.NewGuid();
        var result = StockTransfer.Create(locationId, locationId);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(StockTransferErrors.SameLocation);
    }

    [Fact(DisplayName = "Ship should transition to InTransit and raise event")]
    public void Ship_Should_ChangeStatus()
    {
        // Arrange
        var transfer = StockTransfer.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        transfer.AddItem(Guid.NewGuid(), 5);

        // Act
        var result = transfer.Ship();

        // Assert
        result.IsError.Should().BeFalse();
        transfer.Status.Should().Be(StockTransferStatus.InTransit);
        transfer.DomainEvents.Should().ContainSingle(e => e is StockTransferEvents.StockTransferShipped);
    }

    [Fact(DisplayName = "Ship should fail if transfer is empty")]
    public void Ship_ShouldFail_IfEmpty()
    {
        var transfer = StockTransfer.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var result = transfer.Ship();
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(StockTransferErrors.EmptyTransfer);
    }
}
