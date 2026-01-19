using ReSys.Core.Domain.Settings.ShippingMethods;

namespace ReSys.Core.UnitTests.Domain.Settings.ShippingMethods;

[Trait("Category", "Unit")]
[Trait("Module", "Settings")]
[Trait("Domain", "ShippingMethod")]
public class ShippingMethodTests
{
    [Fact(DisplayName = "Create: Should successfully initialize shipping method")]
    public void Create_Should_InitializeShippingMethod()
    {
        // Act
        var result = ShippingMethod.Create("DHL", "DHL Express", ShippingMethod.ShippingType.Express, 15.00m, "Next day delivery", 1);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("DHL");
        result.Value.BaseCost.Should().Be(15.00m);
        result.Value.Status.Should().Be(ShippingMethod.ShippingStatus.Active);
        result.Value.DomainEvents.Should().ContainSingle(e => e is ShippingMethodEvents.ShippingMethodCreated);
    }

    [Fact(DisplayName = "Create: Should fail if cost is negative")]
    public void Create_ShouldFail_IfCostNegative()
    {
        // Act
        var result = ShippingMethod.Create("Free", "Free", ShippingMethod.ShippingType.FreeShipping, -1.00m);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ShippingMethodErrors.CostNegative);
    }

    [Fact(DisplayName = "UpdateDetails: Should change properties")]
    public void UpdateDetails_Should_ChangeProperties()
    {
        // Arrange
        var method = ShippingMethod.Create("Post", "Postal Service", ShippingMethod.ShippingType.Standard, 5.00m).Value;
        method.ClearDomainEvents();

        // Act
        var result = method.UpdateDetails("New Post", "Mail", 6.00m, "Slower", 2);

        // Assert
        result.IsError.Should().BeFalse();
        method.Name.Should().Be("New Post");
        method.BaseCost.Should().Be(6.00m);
        method.DomainEvents.Should().ContainSingle(e => e is ShippingMethodEvents.ShippingMethodUpdated);
    }

    [Fact(DisplayName = "Delete: Should set soft delete state")]
    public void Delete_Should_SetSoftDeleted()
    {
        // Arrange
        var method = ShippingMethod.Create("Delete", "Delete", ShippingMethod.ShippingType.Pickup, 0).Value;

        // Act
        method.Delete();

        // Assert
        method.IsDeleted.Should().BeTrue();
        method.DomainEvents.Should().Contain(e => e is ShippingMethodEvents.ShippingMethodDeleted);
    }
}