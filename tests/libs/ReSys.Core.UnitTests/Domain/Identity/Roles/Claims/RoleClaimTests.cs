using ReSys.Core.Domain.Identity.Roles.Claims;

namespace ReSys.Core.UnitTests.Domain.Identity.Roles.Claims;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "RoleClaim")]
public class RoleClaimTests
{
    private readonly string _roleId = Guid.NewGuid().ToString();

    [Fact(DisplayName = "Create: Should successfully initialize role claim")]
    public void Create_Should_InitializeRoleClaim()
    {
        // Act
        var result = RoleClaim.Create(_roleId, "Permission", "Write", "System");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RoleId.Should().Be(_roleId);
        result.Value.ClaimType.Should().Be("Permission");
        result.Value.ClaimValue.Should().Be("Write");
        result.Value.AssignedBy.Should().Be("System");
        result.Value.AssignedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Create: Should fail if role id is missing")]
    public void Create_ShouldFail_IfRoleIdMissing()
    {
        // Act
        var result = RoleClaim.Create("", "Type");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RoleClaimErrors.RoleIdRequired);
    }

    [Fact(DisplayName = "Create: Should fail if assigned by too long")]
    public void Create_ShouldFail_IfAssignedByTooLong()
    {
        // Arrange
        var longBy = new string('A', RoleClaimConstraints.AssignedByMaxLength + 1);

        // Act
        var result = RoleClaim.Create(_roleId, "Type", "Val", longBy);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RoleClaimErrors.AssignedByTooLong(RoleClaimConstraints.AssignedByMaxLength));
    }
}