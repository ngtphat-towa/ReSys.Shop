using ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;

namespace ReSys.Core.UnitTests.Domain.Identity.UserGroups.UserGroupMemberships;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "UserGroupMembership")]
public class UserGroupMembershipTests
{
    [Fact(DisplayName = "Create: Should successfully initialize membership")]
    public void Create_Should_InitializeMembership()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var groupId = Guid.NewGuid();
        var assignedBy = "Admin";

        // Act
        var result = UserGroupMembership.Create(userId: userId, groupId: groupId, assignedBy: assignedBy, isPrimary: true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UserId.Should().Be(userId);
        result.Value.UserGroupId.Should().Be(groupId);
        result.Value.AssignedBy.Should().Be(assignedBy);
        result.Value.IsPrimary.Should().Be(true);
        result.Value.JoinedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
