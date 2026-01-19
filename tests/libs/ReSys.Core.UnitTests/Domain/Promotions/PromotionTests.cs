using ReSys.Core.Domain.Promotions.Promotions;
using ErrorOr;

namespace ReSys.Core.UnitTests.Domain.Promotions;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "Promotion")]
public class PromotionTests
{
    [Fact(DisplayName = "Create: Should successfully initialize promotion")]
    public void Create_Should_InitializePromotion()
    {
        // Act
        var result = Promotion.Create("Winter Sale", "WINTER20", "20% off for winter", 100);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Winter Sale");
        result.Value.PromotionCode.Should().Be("WINTER20");
        result.Value.UsageLimit.Should().Be(100);
        result.Value.Active.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is PromotionEvents.PromotionCreated);
    }

    [Fact(DisplayName = "CheckEligibility: Should validate active status")]
    public void CheckEligibility_Should_ValidateActiveStatus()
    {
        // Arrange
        var promo = Promotion.Create("Inactive").Value;
        promo.Active = false;

        // Act
        var result = promo.CheckEligibility(DateTimeOffset.UtcNow);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.NumericType.Should().Be((int)ErrorType.NotFound);
    }

    [Fact(DisplayName = "CheckEligibility: Should validate date range")]
    public void CheckEligibility_Should_ValidateDates()
    {
        // Arrange
        var promo = Promotion.Create("Future").Value;
        promo.StartsAt = DateTimeOffset.UtcNow.AddDays(1);
        promo.ExpiresAt = DateTimeOffset.UtcNow.AddDays(2);

        // Act: Before start
        var resultEarly = promo.CheckEligibility(DateTimeOffset.UtcNow);
        
        // Assert: Too early
        resultEarly.IsError.Should().BeTrue();
        resultEarly.FirstError.Code.Should().Be("Promotion.InvalidCode");

        // Act: After expiration
        var resultLate = promo.CheckEligibility(DateTimeOffset.UtcNow.AddDays(3));

        // Assert: Expired
        resultLate.IsError.Should().BeTrue();
        resultLate.FirstError.Code.Should().Be("Promotion.Expired");
    }

    [Fact(DisplayName = "CheckEligibility: Should validate usage limits")]
    public void CheckEligibility_Should_ValidateUsageLimit()
    {
        // Arrange
        var promo = Promotion.Create("Limited", usageLimit: 1).Value;
        promo.UsageCount = 1;

        // Act
        var result = promo.CheckEligibility(DateTimeOffset.UtcNow);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Promotion.UsageLimitReached");
    }

    [Fact(DisplayName = "IncrementUsage: Should increase count and raise event")]
    public void IncrementUsage_Should_WorkIfEligible()
    {
        // Arrange
        var promo = Promotion.Create("Active").Value;

        // Act
        var result = promo.IncrementUsage();

        // Assert
        result.IsError.Should().BeFalse();
        promo.UsageCount.Should().Be(1);
        promo.DomainEvents.Should().Contain(e => e is PromotionEvents.PromotionUsed);
    }
}
