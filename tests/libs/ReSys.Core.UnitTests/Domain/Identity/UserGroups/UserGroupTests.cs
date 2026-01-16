using ReSys.Core.Domain.Identity.UserGroups;

namespace ReSys.Core.UnitTests.Domain.Identity.UserGroups;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
public class UserGroupTests
{
    [Fact(DisplayName = "Create should succeed with valid data")]
    public void Create_ShouldSucceed_WithValidData()
    {
        // Act
        var result = UserGroup.Create("Admins", "admin_group", "System Administrators", isDefault: true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Admins");
        result.Value.Code.Should().Be("admin_group");
        result.Value.Description.Should().Be("System Administrators");
        result.Value.IsDefault.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is UserGroupEvents.UserGroupCreated);
    }

    [Fact(DisplayName = "Create should fail if code is missing")]
    public void Create_ShouldFail_IfCodeMissing()
    {
        // Act
        var result = UserGroup.Create("Admins", "");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserGroupErrors.CodeRequired);
    }

    [Fact(DisplayName = "Create should fail if name is missing")]
    public void Create_ShouldFail_IfNameMissing()
    {
        // Act
        var result = UserGroup.Create("", "admin_group");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserGroupErrors.NameRequired);
    }

    [Fact(DisplayName = "Create should normalize code")]
    public void Create_ShouldNormalize_Code()
    {
        // Act
        var result = UserGroup.Create("Admins", " ADMIN_GROUP ");

        // Assert
        result.Value.Code.Should().Be("admin_group");
    }
}
