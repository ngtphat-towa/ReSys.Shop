using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.UnitTests.Domain.Inventories.Locations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Domain", "StockLocation")]
public class StockLocationTests
{
    private readonly Address _testAddress = Address.Create("Street", "City", "12345", "US").Value;

    [Fact(DisplayName = "Create: Should successfully initialize location")]
    public void Create_Should_InitializeLocation()
    {
        // Act
        var result = StockLocation.Create("Main Warehouse", "WH-001", _testAddress);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Main Warehouse");
        result.Value.Code.Should().Be("WH-001");
        result.Value.Address.Should().Be(_testAddress);
        result.Value.Active.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is StockLocationEvents.StockLocationCreated);
    }

    [Fact(DisplayName = "Create: Should fail if code format is invalid")]
    public void Create_ShouldFail_IfCodeInvalid()
    {
        // Act
        var result = StockLocation.Create("Store", "invalid code!", _testAddress);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(StockLocationErrors.InvalidCodeFormat);
    }

    [Fact(DisplayName = "Update: Should change properties and raise event")]
    public void Update_Should_ChangeProperties()
    {
        // Arrange
        var location = StockLocation.Create("Old", "OLD", _testAddress).Value;
        var newAddress = Address.Create("Other", "City", "54321", "US").Value;
        location.ClearDomainEvents();

        // Act
        var result = location.Update("New", "New Pres", "NEW-01", true, StockLocationType.RetailStore, newAddress);

        // Assert
        result.IsError.Should().BeFalse();
        location.Name.Should().Be("New");
        location.Code.Should().Be("NEW-01");
        location.Address.Should().Be(newAddress);
        location.IsDefault.Should().BeTrue();
        location.Type.Should().Be(StockLocationType.RetailStore);
        location.DomainEvents.Should().ContainSingle(e => e is StockLocationEvents.StockLocationUpdated);
    }

    [Fact(DisplayName = "IsFulfillable: Should correctly identify fulfillment capability")]
    public void IsFulfillable_Should_WorkCorrectly()
    {
        // Arrange & Act 1: Active Warehouse
        var warehouse = StockLocation.Create("WH", "WH", _testAddress, type: StockLocationType.Warehouse).Value;
        
        // Assert 1
        warehouse.IsFulfillable.Should().BeTrue();

        // Act 2: Inactive
        warehouse.Deactivate();
        
        // Assert 2
        warehouse.IsFulfillable.Should().BeFalse();

        // Act 3: Active but Transit
        var transit = StockLocation.Create("TR", "TR", _testAddress, type: StockLocationType.Transit).Value;
        
        // Assert 3
        transit.IsFulfillable.Should().BeFalse();
    }

    [Fact(DisplayName = "Deactivate: Should fail for default location")]
    public void Deactivate_ShouldFail_IfDefault()
    {
        // Arrange
        var location = StockLocation.Create("Main", "MAIN", _testAddress, isDefault: true).Value;

        // Act
        var result = location.Deactivate();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(StockLocationErrors.CannotDeactivateDefault);
    }

    [Fact(DisplayName = "Delete: Should set soft delete state")]
    public void Delete_Should_SetDeleted()
    {
        // Arrange
        var location = StockLocation.Create("Temp", "TEMP", _testAddress).Value;

        // Act
        var result = location.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        location.IsDeleted.Should().BeTrue();
        location.DeletedAt.Should().NotBeNull();
        location.DomainEvents.Should().Contain(e => e is StockLocationEvents.StockLocationDeleted);
    }
}