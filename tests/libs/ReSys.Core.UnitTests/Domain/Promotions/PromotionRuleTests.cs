using ReSys.Core.Domain.Promotions.Rules;
using FluentAssertions;

namespace ReSys.Core.UnitTests.Domain.Promotions;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionRule")]
public class PromotionRuleTests
{
    [Fact(DisplayName = "Create: Should successfully initialize rule")]
    public void Create_Should_InitializeRule()
    {
        // Arrange
        var promoId = Guid.NewGuid();
        var parameters = new RuleParameters { Threshold = 5 };

        // Act
        var result = PromotionRule.Create(promoId, PromotionRule.RuleType.MinimumQuantity, parameters);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.PromotionId.Should().Be(promoId);
        result.Value.Type.Should().Be(PromotionRule.RuleType.MinimumQuantity);
        result.Value.Parameters.Threshold.Should().Be(5);
    }

    [Fact(DisplayName = "Create: Should successfully initialize Group rule")]
    public void Create_Should_InitializeGroupRule()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var parameters = new RuleParameters { TargetIds = [groupId] };

        // Act
        var result = PromotionRule.Create(Guid.NewGuid(), PromotionRule.RuleType.UserGroup, parameters);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Type.Should().Be(PromotionRule.RuleType.UserGroup);
        result.Value.Parameters.TargetIds.Should().Contain(groupId);
    }
}
