using Microsoft.EntityFrameworkCore;

using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Location.Addresses;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Infrastructure.Persistence;
using ReSys.Core.Domain.Settings.Stores;
using ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

namespace ReSys.Core.UnitTests.Features.Inventories.Services.Fulfillment;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Domain", "Fulfillment")]
public class GreedyFulfillmentStrategyTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly GreedyFulfillmentStrategy _sut = new();

    private async Task<Store> CreateStoreAsync(string name, string code)
    {
        var store = Store.Create(name, code, "USD").Value;
        fixture.Context.Set<Store>().Add(store);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return store;
    }

    [Fact(DisplayName = "CreatePlanAsync: Should fulfill from single location when stock is available")]
    public async Task CreatePlanAsync_ShouldFulfillFromSingleLocation_WhenStockIsAvailable()
    {
        // Arrange
        var store = await CreateStoreAsync("Store 1", "S1");
        var variantId = Guid.NewGuid();
        var requestedItems = new Dictionary<Guid, int> { { variantId, 5 } };

        var address = Address.Create("Test St", "NYC", "10001", "US").Value;
        var location = StockLocation.Create("Main", "WH01", address, isDefault: true).Value;
        var stockItem = StockItem.Create(variantId, location.Id, "SKU01", 10).Value;

        fixture.Context.Set<StockLocation>().Add(location);
        fixture.Context.Set<StockItem>().Add(stockItem);

        store.AddLocation(location);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.CreatePlanAsync(fixture.Context, store.Id, requestedItems, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Shipments.Should().HaveCount(1);
        result.Value.Shipments[0].StockLocationId.Should().Be(location.Id);
        result.Value.Shipments[0].Items[0].Quantity.Should().Be(5);
        result.Value.Shipments[0].Items[0].IsBackordered.Should().BeFalse();
        result.Value.IsFullFulfillment.Should().BeTrue();
    }

    [Fact(DisplayName = "CreatePlanAsync: Should split shipment when no single location has full stock")]
    public async Task CreatePlanAsync_ShouldSplitShipment_WhenNoSingleLocationHasFullStock()
    {
        // Arrange
        var store = await CreateStoreAsync("Store 2", "S2");
        var variantId = Guid.NewGuid();
        var requestedItems = new Dictionary<Guid, int> { { variantId, 10 } };

        var address = Address.Create("Test St", "NYC", "10001", "US").Value;
        var loc1 = StockLocation.Create("Loc1", "WH01", address, isDefault: true).Value;
        var loc2 = StockLocation.Create("Loc2", "WH02", address, isDefault: false).Value;

        var stock1 = StockItem.Create(variantId, loc1.Id, "SKU01", 4).Value; // 4 available
        var stock2 = StockItem.Create(variantId, loc2.Id, "SKU01", 10).Value; // 10 available

        fixture.Context.Set<StockLocation>().AddRange(loc1, loc2);
        fixture.Context.Set<StockItem>().AddRange(stock1, stock2);

        store.AddLocation(loc1);
        store.AddLocation(loc2);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.CreatePlanAsync(fixture.Context, store.Id, requestedItems, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Shipments.Should().HaveCount(2);

        var shipment1 = result.Value.Shipments.First(s => s.StockLocationId == loc1.Id);
        var shipment2 = result.Value.Shipments.First(s => s.StockLocationId == loc2.Id);

        shipment1.Items[0].Quantity.Should().Be(4);
        shipment2.Items[0].Quantity.Should().Be(6);
        result.Value.IsFullFulfillment.Should().BeTrue();
    }

    [Fact(DisplayName = "CreatePlanAsync: Should mark as backordered when no stock available anywhere")]
    public async Task CreatePlanAsync_ShouldMarkAsBackordered_WhenNoStockAvailableAnywhere()
    {
        // Arrange
        var store = await CreateStoreAsync("Store 3", "S3");
        var variantId = Guid.NewGuid();
        var requestedItems = new Dictionary<Guid, int> { { variantId, 5 } };

        var address = Address.Create("Test St", "NYC", "10001", "US").Value;
        var location = StockLocation.Create("Main", "WH01", address, isDefault: true).Value;

        fixture.Context.Set<StockLocation>().Add(location);
        store.AddLocation(location);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.CreatePlanAsync(fixture.Context, store.Id, requestedItems, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Shipments.Should().HaveCount(1);
        result.Value.Shipments[0].Items[0].IsBackordered.Should().BeTrue();
        result.Value.Shipments[0].Items[0].Quantity.Should().Be(5);
        result.Value.IsFullFulfillment.Should().BeFalse();
    }

    [Fact(DisplayName = "CreatePlanAsync: Should ignore non-fulfillable locations")]
    public async Task CreatePlanAsync_ShouldIgnoreNonFulfillableLocations()
    {
        // Arrange
        var store = await CreateStoreAsync("Store 4", "S4");
        var variantId = Guid.NewGuid();
        var requestedItems = new Dictionary<Guid, int> { { variantId, 5 } };

        var address = Address.Create("Test St", "NYC", "10001", "US").Value;

        // Loc 1: Non-fulfillable (Damaged)
        var damagedLoc = StockLocation.Create("Damaged", "DMG01", address, type: StockLocationType.Damaged).Value;
        var damagedStock = StockItem.Create(variantId, damagedLoc.Id, "SKU01", 100).Value;

        // Loc 2: Fulfillable (Warehouse) but empty
        var warehouseLoc = StockLocation.Create("Warehouse", "WH01", address, type: StockLocationType.Warehouse).Value;
        var warehouseStock = StockItem.Create(variantId, warehouseLoc.Id, "SKU01", 0).Value;

        fixture.Context.Set<StockLocation>().AddRange(damagedLoc, warehouseLoc);
        fixture.Context.Set<StockItem>().AddRange(damagedStock, warehouseStock);

        store.AddLocation(damagedLoc);
        store.AddLocation(warehouseLoc);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.CreatePlanAsync(fixture.Context, store.Id, requestedItems, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        // Should NOT take from Damaged, so it should result in a Backorder at the Warehouse
        result.Value.Shipments.Should().HaveCount(1);
        result.Value.Shipments[0].Items[0].IsBackordered.Should().BeTrue();
        result.Value.Shipments[0].StockLocationId.Should().Be(warehouseLoc.Id);
    }

    [Fact(DisplayName = "CreatePlanAsync: Should return EmptyOrder error when request is empty")]
    public async Task CreatePlanAsync_ShouldReturnEmptyOrder_WhenRequestIsEmpty()
    {
        // Act
        var result = await _sut.CreatePlanAsync(fixture.Context, Guid.NewGuid(), new Dictionary<Guid, int>(), ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Fulfillment.EmptyOrder");
    }

    [Fact(DisplayName = "CreatePlanAsync: Should return NoFulfillableLocations when no active fulfillable locations exist")]
    public async Task CreatePlanAsync_ShouldReturnError_WhenNoActiveLocations()
    {
        // Arrange: Using a fresh, empty in-memory DB for this specific edge case
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"Empty_{Guid.NewGuid()}")
            .Options;
        using var emptyContext = new TestAppDbContext(options);
        var strategy = new GreedyFulfillmentStrategy();

        var store = Store.Create("Empty Store", "ES1", "USD").Value;
        emptyContext.Set<Store>().Add(store);

        var address = Address.Create("Street", "City", "12345", "US").Value;

        // Loc 1: Transit (Not fulfillable)
        var loc1 = StockLocation.Create("Transit 1", "TRN1", address, type: StockLocationType.Transit).Value;

        // Loc 2: Damaged (Not fulfillable)
        var loc2 = StockLocation.Create("Damaged 1", "DMG1", address, type: StockLocationType.Damaged).Value;

        emptyContext.Set<StockLocation>().AddRange(loc1, loc2);
        store.AddLocation(loc1);
        store.AddLocation(loc2);
        await emptyContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await strategy.CreatePlanAsync(emptyContext, store.Id, new Dictionary<Guid, int> { { Guid.NewGuid(), 1 } }, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Fulfillment.NoFulfillableLocations");
    }

    [Fact(DisplayName = "CreatePlanAsync: Should prioritize default location when multiple have stock")]
    public async Task CreatePlanAsync_ShouldPrioritizeDefaultLocation()
    {
        // Arrange
        var store = await CreateStoreAsync("Store 5", "S5");
        var variantId = Guid.NewGuid();
        var requestedItems = new Dictionary<Guid, int> { { variantId, 50 } };
        var address = Address.Create("Street", "City", "12345", "US").Value;

        var locNonDefault = StockLocation.Create("Non-Default", "NONDEF", address, isDefault: false).Value;
        var locDefault = StockLocation.Create("Default", "DEF", address, isDefault: true).Value;

        var stockNonDefault = StockItem.Create(variantId, locNonDefault.Id, "SKU", 100).Value;
        var stockDefault = StockItem.Create(variantId, locDefault.Id, "SKU", 100).Value;

        fixture.Context.Set<StockLocation>().AddRange(locNonDefault, locDefault);
        fixture.Context.Set<StockItem>().AddRange(stockNonDefault, stockDefault);

        store.AddLocation(locNonDefault);
        store.AddLocation(locDefault);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.CreatePlanAsync(fixture.Context, store.Id, requestedItems, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Shipments.Should().HaveCount(1);
        result.Value.Shipments[0].StockLocationId.Should().Be(locDefault.Id);
    }
}