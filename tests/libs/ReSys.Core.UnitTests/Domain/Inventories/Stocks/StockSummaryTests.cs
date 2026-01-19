using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.UnitTests.Domain.Inventories.Stocks;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Domain", "StockSummary")]
public class StockSummaryTests
{
    private readonly Guid _variantId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should correctly calculate availability and buyable status")]
    public void Create_Should_InitializeProjection()
    {
        // Act 1: Standard Stock (Not backorderable)
        var summary = StockSummary.Create(_variantId, totalOnHand: 10, totalReserved: 2, backorderable: false);

        // Assert 1
        summary.TotalAvailable.Should().Be(8);
        summary.IsBuyable.Should().BeTrue();

        // Act 2: Out of Stock (Not backorderable)
        var oos = StockSummary.Create(_variantId, totalOnHand: 2, totalReserved: 5, backorderable: false);

        // Assert 2
        oos.TotalAvailable.Should().Be(-3);
        oos.IsBuyable.Should().BeFalse();

        // Act 3: Out of Stock (Backorderable)
        var backorder = StockSummary.Create(_variantId, totalOnHand: 0, totalReserved: 5, backorderable: true);

        // Assert 3
        backorder.TotalAvailable.Should().Be(-5);
        backorder.IsBuyable.Should().BeTrue();
    }

    [Fact(DisplayName = "Update: Should synchronize new values from ledger")]
    public void Update_Should_SyncCalculations()
    {
        // Arrange
        var summary = StockSummary.Create(_variantId, 10, 0, false);

        // Act
        summary.Update(totalOnHand: 5, totalReserved: 5, backorderable: false);

        // Assert
        summary.TotalOnHand.Should().Be(5);
        summary.TotalReserved.Should().Be(5);
        summary.TotalAvailable.Should().Be(0);
        summary.IsBuyable.Should().BeFalse();
    }
}
