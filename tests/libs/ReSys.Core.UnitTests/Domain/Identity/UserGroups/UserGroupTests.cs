using ReSys.Core.Domain.Identity.UserGroups;

namespace ReSys.Core.UnitTests.Domain.Identity.UserGroups;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "UserGroup")]
public class UserGroupTests
{
    [Fact(DisplayName = "Create: Should successfully initialize group")]
    public void Create_Should_InitializeGroup()
    {
        // Act
        var result = UserGroup.Create("Administrators", "admins", "System admins", isDefault: true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Administrators");
        result.Value.Code.Should().Be("ADMINS");
        result.Value.Description.Should().Be("System admins");
        result.Value.IsDefault.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is UserGroupEvents.GroupCreated);
    }

    [Fact(DisplayName = "Create: Should normalize code to uppercase")]
    public void Create_Should_NormalizeCode()
    {
        // Act
        var result = UserGroup.Create("Test", "test_code ");

        // Assert
        result.Value.Code.Should().Be("TEST_CODE");
    }

    [Fact(DisplayName = "Create: Should fail if name is missing")]
    public void Create_ShouldFail_IfNameMissing()
    {
        // Act
        var result = UserGroup.Create("", "code");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserGroupErrors.NameRequired);
    }
}