using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Domain.Identity.Users.Roles;

namespace ReSys.Core.UnitTests.Domain.Identity.Roles;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "Role")]
public class RoleTests
{
    [Fact(DisplayName = "Create: Should successfully initialize role")]
    public void Create_Should_InitializeRole()
    {
        // Act
        var result = Role.Create("Admin", "Administrator", "System admin role", 10);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Admin");
        result.Value.DisplayName.Should().Be("Administrator");
        result.Value.Priority.Should().Be(10);
        result.Value.DomainEvents.Should().ContainSingle(e => e is RoleEvents.RoleCreated);
    }

    [Fact(DisplayName = "Create: Should fail if name is empty")]
    public void Create_ShouldFail_IfNameEmpty()
    {
        // Act
        var result = Role.Create("");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RoleErrors.NameRequired);
    }

    [Fact(DisplayName = "Create: Should clamp priority")]
    public void Create_Should_ClampPriority()
    {
        // Act
        var result = Role.Create("Test", priority: 200);

        // Assert
        result.Value.Priority.Should().Be(RoleConstraints.MaxPriority);
    }

    [Fact(DisplayName = "Update: Should change properties and raise event")]
    public void Update_Should_ChangeProperties()
    {
        // Arrange
        var role = Role.Create("Editor").Value;

        // Act
        var result = role.Update("Content Editor", "Allows editing content", 5);

        // Assert
        result.IsError.Should().BeFalse();
        role.DisplayName.Should().Be("Content Editor");
        role.Description.Should().Be("Allows editing content");
        role.Priority.Should().Be(5);
        role.DomainEvents.Should().Contain(e => e is RoleEvents.RoleUpdated);
    }

    [Fact(DisplayName = "Update: Should fail for default roles")]
    public void Update_ShouldFail_ForDefaultRoles()
    {
        // Arrange
        var role = Role.Create("Guest", isDefault: true).Value;

        // Act
        var result = role.Update("New Display Name", "New Description");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RoleErrors.CannotModifyDefaultRole);
    }

    [Fact(DisplayName = "Delete: Should succeed for regular roles")]
    public void Delete_Should_Succeed_ForRegularRoles()
    {
        // Arrange
        var role = Role.Create("Temporary").Value;

        // Act
        var result = role.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        role.DomainEvents.Should().Contain(e => e is RoleEvents.RoleDeleted);
    }

    [Fact(DisplayName = "Delete: Should fail for system roles")]
    public void Delete_ShouldFail_ForSystemRoles()
    {
        // Arrange
        var role = Role.Create("SuperAdmin", isSystemRole: true).Value;

        // Act
        var result = role.Delete();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RoleErrors.SystemRoleProtected);
    }

    [Fact(DisplayName = "Delete: Should fail if role in use")]
    public void Delete_ShouldFail_IfRoleInUse()
    {
        // Arrange
        var role = Role.Create("InUse").Value;
        role.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = "some-user" });

        // Act
        var result = role.Delete();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Role.RoleInUse");
    }
}
