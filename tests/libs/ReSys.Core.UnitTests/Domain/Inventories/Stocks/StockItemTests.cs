using ReSys.Core.Domain.Ordering.InventoryUnits;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.UnitTests.Domain.Inventories.Stocks;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Domain", "StockItem")]
public class StockItemTests
{
    private readonly Address _testAddress = Address.Create("Street", "City", "12345", "US").Value;

    [Fact(DisplayName = "Create: Should successfully initialize stock item")]
    public void Create_Should_InitializeStockItem()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        // Act
        var result = StockItem.Create(variantId, locationId, "SKU01", 10, 50.5m);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.VariantId.Should().Be(variantId);
        result.Value.StockLocationId.Should().Be(locationId);
        result.Value.QuantityOnHand.Should().Be(10);
        result.Value.StockMovements.Should().HaveCount(1);
        result.Value.StockMovements.First().Type.Should().Be(StockMovementType.Receipt);
        result.Value.StockMovements.First().UnitCost.Should().Be(50.5m);
    }

    [Fact(DisplayName = "AdjustStock: Should update quantity and record movement")]
    public void AdjustStock_Should_UpdateBalance()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var location = StockLocation.Create("Main", "WH01", _testAddress).Value;
        var stockItem = StockItem.Create(variantId, location.Id, "SKU01", 10).Value;

        // Act
        var result = stockItem.AdjustStock(5, StockMovementType.Receipt, 100, "Monthly replenishment");

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.QuantityOnHand.Should().Be(15);
        stockItem.StockMovements.Should().HaveCount(2); // Initial + New

        var latest = stockItem.StockMovements.Last();
        latest.Quantity.Should().Be(5);
        latest.BalanceBefore.Should().Be(10);
        latest.BalanceAfter.Should().Be(15);
        latest.UnitCost.Should().Be(100);
    }

    [Fact(DisplayName = "AdjustStock: Should backfill waiting orders when new stock arrives")]
    public void AdjustStock_Should_BackfillWaitingOrders_WhenStockArrives()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var location = StockLocation.Create("Main", "WH01", _testAddress).Value;
        var stockItem = StockItem.Create(variantId, location.Id, "SKU01", 0).Value;
        stockItem.SetBackorderPolicy(true, 100);

        // 1. Create a backorder
        var orderId = Guid.NewGuid();
        var lineItemId = Guid.NewGuid();
        stockItem.Reserve(2, orderId, lineItemId);

        stockItem.CountAvailable.Should().Be(-2);
        stockItem.QuantityReserved.Should().Be(2); // Total promise is 2

        // Act: Add 5 items
        stockItem.AdjustStock(5, StockMovementType.Receipt);

        // Assert
        stockItem.QuantityOnHand.Should().Be(5);
        stockItem.QuantityReserved.Should().Be(2); // Promise count stays same, just promoted to OnHand
        stockItem.CountAvailable.Should().Be(3);

        stockItem.InventoryUnits.Count(u => u.State == InventoryUnitState.OnHand).Should().Be(2);
    }

    [Fact(DisplayName = "AdjustStock: Should fail if backorder limit exceeded")]
    public void AdjustStock_ShouldFail_IfBackorderLimitExceeded()
    {
        // Arrange
        var stockItem = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU01", 0).Value;
        stockItem.SetBackorderPolicy(true, 10);

        // Act
        var result = stockItem.AdjustStock(-15, StockMovementType.Adjustment);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StockItem.BackorderLimitExceeded");
    }

    [Fact(DisplayName = "Reserve: Should handle both OnHand and Backordered states correctly")]
    public void Reserve_Should_HandleBothStatesCorrectly()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var location = StockLocation.Create("Main", "WH01", _testAddress).Value;
        var stockItem = StockItem.Create(variantId, location.Id, "SKU01", 2).Value;
        stockItem.SetBackorderPolicy(true, 100);

        // Act: Reserve 5 (2 OnHand, 3 Backordered)
        var orderId = Guid.NewGuid();
        var lineItemId = Guid.NewGuid();
        var result = stockItem.Reserve(5, orderId, lineItemId);

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.QuantityReserved.Should().Be(5); // Total promised is 5
        stockItem.CountAvailable.Should().Be(-3); // 2 physical - 5 promised

        stockItem.InventoryUnits.Should().HaveCount(5);
        stockItem.InventoryUnits.Count(u => u.State == InventoryUnitState.OnHand).Should().Be(2);
        stockItem.InventoryUnits.Count(u => u.State == InventoryUnitState.Backordered).Should().Be(3);
    }

    [Fact(DisplayName = "Reserve: Should fail when stock empty and backordering is disabled")]
    public void Reserve_Should_Fail_WhenNoBackorderAllowed()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var location = StockLocation.Create("Main", "WH01", _testAddress).Value;
        var stockItem = StockItem.Create(variantId, location.Id, "SKU01", 0).Value;
        stockItem.SetBackorderPolicy(false, 0);

        // Act
        var result = stockItem.Reserve(1, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StockItem.InsufficientStock");
    }

    [Fact(DisplayName = "Release: Should return stock to availability")]
    public void Release_Should_ReturnStockToAvailability()
    {
        // Arrange
        var stockItem = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU01", 10).Value;
        var orderId = Guid.NewGuid();
        stockItem.Reserve(3, orderId, Guid.NewGuid());
        stockItem.QuantityReserved.Should().Be(3);

        // Act
        var result = stockItem.Release(2, orderId);

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.QuantityReserved.Should().Be(1);
        stockItem.InventoryUnits.Count(u => u.State == InventoryUnitState.Canceled).Should().Be(2);
    }

    [Fact(DisplayName = "Fulfill: Should strictly ship only OnHand items")]
    public void Fulfill_Should_OnlyShipOnHandItems()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var location = StockLocation.Create("Main", "WH01", _testAddress).Value;
        var stockItem = StockItem.Create(variantId, location.Id, "SKU01", 10).Value;

        var orderId = Guid.NewGuid();
        var lineItemId = Guid.NewGuid();
        stockItem.Reserve(5, orderId, lineItemId);

        // Act
        var shipmentId = Guid.NewGuid();
        var result = stockItem.Fulfill(5, shipmentId, "INV-001");

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.QuantityOnHand.Should().Be(5);
        stockItem.QuantityReserved.Should().Be(0);
        stockItem.InventoryUnits.Should().OnlyContain(u => u.State == InventoryUnitState.Shipped);
    }

    [Fact(DisplayName = "Fulfill: Should allow direct sale (fulfill without reservation) when physical stock exists")]
    public void Fulfill_Should_AllowDirectSale_WhenPhysicalStockExists()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var location = StockLocation.Create("Main", "WH01", _testAddress).Value;
        var stockItem = StockItem.Create(variantId, location.Id, "SKU01", 10).Value;

        // Act: Attempt to fulfill WITHOUT reservation
        var result = stockItem.Fulfill(1, Guid.NewGuid(), "POS-REF");

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.QuantityOnHand.Should().Be(9);
        stockItem.InventoryUnits.Should().ContainSingle(u => u.State == InventoryUnitState.Shipped);
    }

    [Fact(DisplayName = "Fulfill: Should fail when physical stock empty and backordering is disabled")]
    public void Fulfill_Should_Fail_WhenPhysicalStockEmptyAndNoBackorder()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var location = StockLocation.Create("Main", "WH01", _testAddress).Value;
        var stockItem = StockItem.Create(variantId, location.Id, "SKU01", 0).Value;
        stockItem.SetBackorderPolicy(false, 0);

        // Act
        var result = stockItem.Fulfill(1, Guid.NewGuid(), "INV-REF");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StockItem.InsufficientStock");
    }

    [Fact(DisplayName = "Delete: Should set soft delete state")]
    public void Delete_Should_SetDeleted()
    {
        // Arrange
        var stockItem = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU01", 10).Value;

        // Act
        var result = stockItem.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.IsDeleted.Should().BeTrue();
        stockItem.DeletedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Restore: Should clear soft delete state")]
    public void Restore_Should_ClearDeleted()
    {
        // Arrange
        var stockItem = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU01", 10).Value;
        stockItem.Delete();

        // Act
        var result = stockItem.Restore();

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.IsDeleted.Should().BeFalse();
        stockItem.DeletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "SetBackorderPolicy: Should update limits")]
    public void SetBackorderPolicy_Should_UpdateLimits()
    {
        // Arrange
        var stockItem = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU01", 10).Value;

        // Act
        var result = stockItem.SetBackorderPolicy(false, 50);

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.Backorderable.Should().BeFalse();
        stockItem.BackorderLimit.Should().Be(50);
    }
}
