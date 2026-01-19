using ReSys.Core.Domain.Settings.Stores;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.UnitTests.Domain.Settings.Stores;

[Trait("Category", "Unit")]
[Trait("Module", "Settings")]
[Trait("Domain", "Store")]
public class StoreTests
{
    private readonly Address _testAddress = Address.Create("123 St", "City", "12345", "US").Value;

    [Fact(DisplayName = "Create: Should successfully initialize store")]
    public void Create_Should_InitializeStore()
    {
        // Act
        var result = Store.Create("Main Store", "MAIN_US", "USD", "https://shop.example.com");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Main Store");
        result.Value.Code.Should().Be("MAIN_US");
        result.Value.DefaultCurrency.Should().Be("USD");
        result.Value.Url.Should().Be("https://shop.example.com");
        result.Value.DomainEvents.Should().ContainSingle(e => e is StoreEvents.StoreCreated);
    }

    [Fact(DisplayName = "Create: Should normalize code and currency")]
    public void Create_Should_NormalizeInputs()
    {
        // Act
        var result = Store.Create("Store", " store_01 ", " usd ");

        // Assert
        result.Value.Code.Should().Be("STORE_01");
        result.Value.DefaultCurrency.Should().Be("USD");
    }

    [Fact(DisplayName = "Create: Should fail for invalid code format")]
    public void Create_ShouldFail_ForInvalidCode()
    {
        // Act
        var result = Store.Create("Store", "INVALID-CODE", "USD");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(StoreErrors.InvalidCodeFormat);
    }

    [Fact(DisplayName = "Update: Should change properties and raise event")]
    public void Update_Should_ChangeProperties()
    {
        // Arrange
        var store = Store.Create("Old Name", "CODE", "USD").Value;
        store.ClearDomainEvents();

        // Act
        var result = store.Update("New Name", "https://new.url", "EUR", true, "lb");

        // Assert
        result.IsError.Should().BeFalse();
        store.Name.Should().Be("New Name");
        store.Url.Should().Be("https://new.url");
        store.DefaultCurrency.Should().Be("EUR");
        store.PricesIncludeTax.Should().BeTrue();
        store.DefaultWeightUnit.Should().Be("lb");
        store.DomainEvents.Should().ContainSingle(e => e is StoreEvents.StoreUpdated);
    }

    [Fact(DisplayName = "AddLocation: Should link a location to the store")]
    public void AddLocation_Should_LinkLocation()
    {
        // Arrange
        var store = Store.Create("Store", "S1", "USD").Value;
        var location = StockLocation.Create("Warehouse", "WH1", _testAddress).Value;

        // Act
        var result = store.AddLocation(location);

        // Assert
        result.IsError.Should().BeFalse();
        store.StoreStockLocations.Should().Contain(x => x.StockLocationId == location.Id);
    }

    [Fact(DisplayName = "SetDefaultLocation: Should fail if location is not linked")]
    public void SetDefaultLocation_ShouldFail_IfLocationNotLinked()
    {
        // Arrange
        var store = Store.Create("Store", "S1", "USD").Value;
        var unlinkedLocationId = Guid.NewGuid();

        // Act
        store.SetDefaultLocation(unlinkedLocationId);

        // Assert
        // Business Rule: Can only default to a location we actually use.
        store.DefaultStockLocationId.Should().BeNull();
    }

    [Fact(DisplayName = "SetDefaultLocation: Should succeed if location is linked")]
    public void SetDefaultLocation_Should_WorkIfLinked()
    {
        // Arrange
        var store = Store.Create("Store", "S1", "USD").Value;
        var location = StockLocation.Create("Warehouse", "WH1", _testAddress).Value;
        store.AddLocation(location);

        // Act
        store.SetDefaultLocation(location.Id);

        // Assert
        store.DefaultStockLocationId.Should().Be(location.Id);
    }

    [Fact(DisplayName = "RemoveLocation: Should fail if removing the default location")]
    public void RemoveLocation_ShouldFail_IfDefault()
    {
        // Arrange
        var store = Store.Create("Store", "S1", "USD").Value;
        var location = StockLocation.Create("Warehouse", "WH1", _testAddress).Value;
        store.AddLocation(location);
        store.SetDefaultLocation(location.Id);

        // Act
        var result = store.RemoveLocation(location.Id);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(StoreErrors.CannotRemoveDefaultLocation);
    }

    [Fact(DisplayName = "Delete: Should set soft delete state")]
    public void Delete_Should_SetSoftDeleted()
    {
        // Arrange
        var store = Store.Create("Store", "S1", "USD").Value;

        // Act
        var result = store.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        store.IsDeleted.Should().BeTrue();
        store.DeletedAt.Should().NotBeNull();
        store.DomainEvents.Should().Contain(e => e is StoreEvents.StoreDeleted);
    }
}
