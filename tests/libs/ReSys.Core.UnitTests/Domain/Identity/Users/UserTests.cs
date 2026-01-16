using ReSys.Core.Domain.Identity.Users;

namespace ReSys.Core.UnitTests.Domain.Identity.Users;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
public class UserTests
{
    [Fact(DisplayName = "Create should succeed with valid email")]
    public void Create_ShouldSucceed_WithValidEmail()
    {
        // Act
        var result = User.Create("test@example.com", "John", "Doe");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Email.Should().Be("test@example.com");
        result.Value.FullName.Should().Be("John Doe");
        result.Value.IsActive.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is UserEvents.UserCreated);
    }

    [Theory(DisplayName = "Create should fail with invalid email formats")]
    [InlineData("invalid-email")]
    [InlineData("missingat.com")]
    [InlineData("@missinguser.com")]
    [InlineData("user@.com")]
    public void Create_ShouldFail_WithInvalidEmail(string invalidEmail)
    {
        // Act
        var result = User.Create(invalidEmail);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact(DisplayName = "Create should fail when email is empty")]
    public void Create_ShouldFail_WhenEmailEmpty()
    {
        var result = User.Create("");
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.EmailRequired);
    }

    [Fact(DisplayName = "EnsureCustomerProfile should create profile if missing")]
    public void EnsureCustomerProfile_ShouldCreateProfile()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        user.CustomerProfile.Should().BeNull();

        // Act
        user.EnsureCustomerProfile();

        // Assert
        user.CustomerProfile.Should().NotBeNull();
        user.CustomerProfile!.UserId.Should().Be(user.Id);
    }

    [Fact(DisplayName = "EnsureCustomerProfile should be idempotent")]
    public void EnsureCustomerProfile_ShouldBeIdempotent()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        user.EnsureCustomerProfile();
        var originalProfile = user.CustomerProfile;

        // Act
        user.EnsureCustomerProfile();

        // Assert
        user.CustomerProfile.Should().BeSameAs(originalProfile);
    }

    [Fact(DisplayName = "EnsureStaffProfile should create profile with employee ID")]
    public void EnsureStaffProfile_ShouldCreateProfile()
    {
        // Arrange
        var user = User.Create("staff@example.com").Value;
        var empId = "EMP-001";

        // Act
        user.EnsureStaffProfile(empId);

        // Assert
        user.StaffProfile.Should().NotBeNull();
        user.StaffProfile!.EmployeeId.Should().Be(empId);
        user.StaffProfile.UserId.Should().Be(user.Id);
    }

    [Fact(DisplayName = "AddToGroup should add membership correctly")]
    public void AddToGroup_ShouldAddMembership()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        var groupId = Guid.NewGuid();

        // Act
        user.AddToGroup(groupId, "Admin", isPrimary: true);

        // Assert
        user.GroupMemberships.Should().HaveCount(1);
        var membership = user.GroupMemberships.First();
        membership.UserGroupId.Should().Be(groupId);
        membership.AssignedBy.Should().Be("Admin");
        membership.IsPrimary.Should().BeTrue();
    }

    [Fact(DisplayName = "AddToGroup should ignore duplicate additions")]
    public void AddToGroup_ShouldNotDuplicate()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        var groupId = Guid.NewGuid();
        user.AddToGroup(groupId);

        // Act
        user.AddToGroup(groupId);

        // Assert
        user.GroupMemberships.Should().HaveCount(1);
    }

    [Fact(DisplayName = "RemoveFromGroup should remove existing membership")]
    public void RemoveFromGroup_ShouldRemove_ExistingMembership()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        var groupId = Guid.NewGuid();
        user.AddToGroup(groupId);

        // Act
        user.RemoveFromGroup(groupId);

        // Assert
        user.GroupMemberships.Should().BeEmpty();
    }

    [Fact(DisplayName = "RemoveFromGroup should do nothing if membership not found")]
    public void RemoveFromGroup_ShouldDoNothing_IfNotFound()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        var groupId = Guid.NewGuid();

        // Act
        user.RemoveFromGroup(groupId);

        // Assert (No exception expected)
        user.GroupMemberships.Should().BeEmpty();
    }

    [Fact(DisplayName = "RecordSignIn should update metrics and raise event")]
    public void RecordSignIn_ShouldUpdateMetrics()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        var ip = "127.0.0.1";

        // Act
        user.RecordSignIn(ip);

        // Assert
        user.LastSignInAt.Should().NotBeNull();
        user.LastIpAddress.Should().Be(ip);
        user.SignInCount.Should().Be(1);
        user.DomainEvents.Should().ContainSingle(e => e is UserEvents.UserLoggedIn);
    }
}