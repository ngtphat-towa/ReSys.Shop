using ReSys.Core.Domain.Identity.Permissions;

namespace ReSys.Core.UnitTests.Domain.Identity.Permissions;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "AccessPermission")]
public class AccessPermissionTests
{
    [Fact(DisplayName = "Create: Should successfully initialize permission from segments")]
    public void Create_Should_InitializeFromSegments()
    {
        // Act
        var result = AccessPermission.Create(area: "catalog", resource: "products", action: "read");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("catalog.products.read");
        result.Value.Area.Should().Be("catalog");
        result.Value.Resource.Should().Be("products");
        result.Value.Action.Should().Be("read");
        result.Value.DisplayName.Should().Be("Read Products");
        result.Value.DomainEvents.Should().ContainSingle(e => e is AccessPermissionEvents.PermissionCreated);
    }

    [Fact(DisplayName = "Create: Should successfully initialize permission from full name")]
    public void Create_Should_InitializeFromFullName()
    {
        // Act
        var result = AccessPermission.Create(name: "identity.users.roles.assign");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("identity.users.roles.assign");
        result.Value.Area.Should().Be("identity");
        result.Value.Resource.Should().Be("users.roles");
        result.Value.Action.Should().Be("assign");
    }

    [Theory(DisplayName = "Create: Should fail with invalid formats")]
    [InlineData("a.b")] // Too few segments
    [InlineData("area.res.a")] // Action too short (MinSegmentLength is 2)
    public void Create_ShouldFail_WithInvalidFormat(string invalidName)
    {
        // Act
        var result = AccessPermission.Create(name: invalidName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccessPermissionErrors.InvalidFormat);
    }

    [Fact(DisplayName = "Create: Should fail when resource is missing in full name")]
    public void Create_ShouldFail_WhenResourceMissing()
    {
        // Act
        var result = AccessPermission.Create(name: "area..action");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AccessPermissionErrors.ResourceRequired);
    }

    [Fact(DisplayName = "Update: Should change metadata")]
    public void Update_Should_ChangeMetadata()
    {
        // Arrange
        var permission = AccessPermission.Create(area: "sales", resource: "orders", action: "write").Value;

        // Act
        var result = permission.Update("Create Orders", "Permission to create new sales orders");

        // Assert
        result.IsError.Should().BeFalse();
        permission.DisplayName.Should().Be("Create Orders");
        permission.Description.Should().Be("Permission to create new sales orders");
        permission.DomainEvents.Should().Contain(e => e is AccessPermissionEvents.PermissionUpdated);
    }

    [Fact(DisplayName = "GenerateDisplayName: Should format correctly")]
    public void GenerateDisplayName_Should_FormatCorrectly()
    {
        // Act
        var result = AccessPermission.Create(area: "inventory", resource: "warehouse.stock", action: "adjust");

        // Assert
        result.Value.DisplayName.Should().Be("Adjust Warehouse Stock");
        result.Value.Description.Should().Be("Adjust Warehouse Stock in Inventory area");
    }
}
