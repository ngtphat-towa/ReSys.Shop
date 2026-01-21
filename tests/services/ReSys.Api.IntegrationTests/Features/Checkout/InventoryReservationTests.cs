using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Inventories.Services;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace ReSys.Api.IntegrationTests.Features.Checkout;

[Collection("Shared Database")]
public class InventoryReservationTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    private async Task<(StockLocation Location, Variant Variant)> SetupInventoryAsync(int initialStock)
    {
        var address = Address.Create("123 Test St", "Test City", "12345", "US", "Test", "User").Value;
        var location = StockLocation.Create("Main Warehouse", "WH-" + Guid.NewGuid().ToString()[..8], address).Value;
        
        var productResult = Product.Create("Test Product", "SKU-" + Guid.NewGuid().ToString()[..8], 100);
        var product = productResult.Value;
        var variant = product.MasterVariant!;

        Context.Set<StockLocation>().Add(location);
        Context.Set<Product>().Add(product);
        
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stockItem = StockItem.Create(variant.Id, location.Id, variant.Sku!, initialStock: initialStock).Value;
        if (initialStock == 0)
        {
            stockItem.SetBackorderPolicy(false, 0);
        }
        
        Context.Set<StockItem>().Add(stockItem);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (location, variant);
    }

    [Fact]
    public async Task AttemptReservation_ShouldFail_WhenStockIsInsufficient()
    {
        // 1. Arrange
        var (_, variant) = await SetupInventoryAsync(0);

        // Create Order with Item
        var order = Order.Create(Guid.NewGuid(), "USD", Guid.NewGuid().ToString()).Value;
        order.AddVariant(variant, 1, DateTimeOffset.UtcNow);
        
        Context.Set<Order>().Add(order);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // 2. Act
        // Use the same scope/context to avoid tracking issues in test environment
        var service = Scope.ServiceProvider.GetRequiredService<IInventoryReservationService>();
        var result = await service.AttemptReservationAsync(order.Id, order.LineItems, TestContext.Current.CancellationToken);

        // 3. Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Contain("InsufficientStock");
    }

    [Fact]
    public async Task AttemptReservation_ShouldSucceed_WhenStockIsAvailable()
    {
        // 1. Arrange
        var (_, variant) = await SetupInventoryAsync(10);

        var order = Order.Create(Guid.NewGuid(), "USD", Guid.NewGuid().ToString()).Value;
        order.AddVariant(variant, 1, DateTimeOffset.UtcNow);
        
        Context.Set<Order>().Add(order);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // 2. Act
        var service = Scope.ServiceProvider.GetRequiredService<IInventoryReservationService>();
        var result = await service.AttemptReservationAsync(order.Id, order.LineItems, TestContext.Current.CancellationToken);

        // 3. Assert
        result.IsError.Should().BeFalse();
        
        // Verify DB state
        // Reload explicitly to verify persistence
        ((DbContext)Context).ChangeTracker.Clear();
        var dbStock = await Context.Set<StockItem>()
            .FirstOrDefaultAsync(s => s.VariantId == variant.Id, TestContext.Current.CancellationToken);
            
        dbStock!.QuantityReserved.Should().Be(1);
    }
}
