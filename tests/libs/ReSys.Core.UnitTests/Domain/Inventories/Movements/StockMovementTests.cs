using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.UnitTests.Domain.Inventories.Movements;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Domain", "StockMovement")]
public class StockMovementTests
{
    private readonly Guid _stockItemId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should successfully initialize ledger entry")]
    public void Create_Should_InitializeMovement()
    {
        // Act
        var movement = StockMovement.Create(
            _stockItemId, 
            quantity: 10, 
            balanceBefore: 5, 
            type: StockMovementType.Receipt, 
            unitCost: 100.50m,
            reason: "Replenishment",
            reference: "PO-123");

        // Assert
        movement.StockItemId.Should().Be(_stockItemId);
        movement.Quantity.Should().Be(10);
        movement.BalanceBefore.Should().Be(5);
        movement.BalanceAfter.Should().Be(15);
        movement.Type.Should().Be(StockMovementType.Receipt);
        movement.UnitCost.Should().Be(100.50m);
        movement.Reason.Should().Be("Replenishment");
        movement.Reference.Should().Be("PO-123");
        movement.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Create: Should correctly calculate balance for deductions")]
    public void Create_Should_CalculateNegativeBalance()
    {
        // Act
        var movement = StockMovement.Create(
            _stockItemId, 
            quantity: -3, 
            balanceBefore: 10, 
            type: StockMovementType.Sale);

        // Assert
        movement.Quantity.Should().Be(-3);
        movement.BalanceBefore.Should().Be(10);
        movement.BalanceAfter.Should().Be(7);
    }
}