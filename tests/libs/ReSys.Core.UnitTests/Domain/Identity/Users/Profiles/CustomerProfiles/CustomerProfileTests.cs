using ReSys.Core.Domain.Identity.Users;

namespace ReSys.Core.UnitTests.Domain.Identity.Users.Profiles.CustomerProfiles;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
public class CustomerProfileTests
{
    [Fact(DisplayName = "UpdateMetrics should correctly increment lifetime value and order count")]
    public void UpdateMetrics_ShouldIncrement_LtvAndCount()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        user.EnsureCustomerProfile();
        var profile = user.CustomerProfile!;

        // Act
        profile.UpdateMetrics(100.50m);
        profile.UpdateMetrics(50.00m);

        // Assert
        profile.LifetimeValue.Should().Be(150.50m);
        profile.OrdersCount.Should().Be(2);
        profile.LastOrderAt.Should().NotBeNull();
    }
}
