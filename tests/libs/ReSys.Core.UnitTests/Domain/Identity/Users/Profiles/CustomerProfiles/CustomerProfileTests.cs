using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;

namespace ReSys.Core.UnitTests.Domain.Identity.Users.Profiles.CustomerProfiles;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "CustomerProfile")]
public class CustomerProfileTests
{
    [Fact(DisplayName = "UpdateMetrics: Should correctly increment LTV and order count")]
    public void UpdateMetrics_Should_IncrementMetrics()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        user.EnsureCustomerProfile();
        var profile = user.CustomerProfile!;

        // Act
        profile.UpdateMetrics(100.50m);
        profile.UpdateMetrics(50.25m);

        // Assert
        profile.LifetimeValue.Should().Be(150.75m);
        profile.OrdersCount.Should().Be(2);
        profile.LastOrderAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "SetPreferences: Should update preference flags")]
    public void SetPreferences_Should_UpdateFlags()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        user.EnsureCustomerProfile();
        var profile = user.CustomerProfile!;

        // Act
        profile.SetPreferences(true, "fr-FR", "EUR");

        // Assert
        profile.AcceptsMarketing.Should().BeTrue();
        profile.PreferredLocale.Should().Be("fr-FR");
        profile.PreferredCurrency.Should().Be("EUR");
    }

    [Fact(DisplayName = "SetDemographics: Should update gender")]
    public void SetDemographics_Should_UpdateGender()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        user.EnsureCustomerProfile();
        var profile = user.CustomerProfile!;

        // Act
        profile.SetDemographics("Non-Binary");

        // Assert
        profile.Gender.Should().Be("Non-Binary");
    }
}