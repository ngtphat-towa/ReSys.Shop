using ReSys.Core.Domain.Ordering.Adjustments;

namespace ReSys.Core.UnitTests.Domain.Ordering.Adjustments;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Domain", "Adjustments")]
public class AdjustmentTests
{
    [Fact(DisplayName = "OrderAdjustment: Should successfully initialize and track audit metadata")]
    public void OrderAdjustment_Should_Initialize()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var promoId = Guid.NewGuid();

        // Act
        var result = OrderAdjustment.Create(orderId, -500, "COUPON5", OrderAdjustment.AdjustmentScope.Order, promoId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.OrderId.Should().Be(orderId);
        result.Value.AmountCents.Should().Be(-500);
        result.Value.PromotionId.Should().Be(promoId);
        result.Value.IsPromotion.Should().BeTrue();
        result.Value.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "LineItemAdjustment: Should successfully initialize and update eligibility")]
    public void LineItemAdjustment_Should_Initialize()
    {
        // Arrange
        var lineItemId = Guid.NewGuid();

        // Act
        var result = LineItemAdjustment.Create(lineItemId, -200, "Discount", eligible: true);
        var adjustment = result.Value;
        adjustment.SetEligibility(false);

        // Assert
        result.IsError.Should().BeFalse();
        adjustment.LineItemId.Should().Be(lineItemId);
        adjustment.Eligible.Should().BeFalse();
        adjustment.UpdatedAt.Should().NotBeNull();
    }
}