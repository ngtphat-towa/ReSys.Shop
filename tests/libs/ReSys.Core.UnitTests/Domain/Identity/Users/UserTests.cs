using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Identity.Users.Roles;
using ReSys.Core.Domain.Identity.Tokens;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.UnitTests.Domain.Identity.Users;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "User")]
public class UserTests
{
    private const string ValidEmail = "test@example.com";

    [Fact(DisplayName = "Create: Should successfully initialize user")]
    public void Create_Should_InitializeUser()
    {
        // Act
        var result = User.Create(ValidEmail, "testuser", "John", "Doe");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Email.Should().Be(ValidEmail);
        result.Value.UserName.Should().Be("testuser");
        result.Value.FullName.Should().Be("John Doe");
        result.Value.IsActive.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is UserEvents.UserCreated);
    }

    [Theory(DisplayName = "Create: Should fail with invalid email formats")]
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

    [Fact(DisplayName = "UpdateStatus: Should change active state and raise event")]
    public void UpdateStatus_Should_ChangeState()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;

        // Act
        var result = user.UpdateStatus(false);

        // Assert
        result.IsError.Should().BeFalse();
        user.IsActive.Should().BeFalse();
        user.DomainEvents.Should().Contain(e => e is UserEvents.StatusChanged);
    }

    [Fact(DisplayName = "UpdateProfile: Should update personal info")]
    public void UpdateProfile_Should_UpdateInfo()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var dob = DateTimeOffset.UtcNow.AddYears(-25);

        // Act
        var result = user.UpdateProfile("Jane", "Smith", dob, "/images/jane.png");

        // Assert
        result.IsError.Should().BeFalse();
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.DateOfBirth.Should().Be(dob);
        user.ProfileImagePath.Should().Be("/images/jane.png");
        user.DomainEvents.Should().Contain(e => e is UserEvents.ProfileUpdated);
    }

    [Fact(DisplayName = "RecordSignIn: Should update metrics")]
    public void RecordSignIn_Should_UpdateMetrics()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var ip = "192.168.1.1";

        // Act
        user.RecordSignIn(ip);

        // Assert
        user.CurrentSignInIp.Should().Be(ip);
        user.SignInCount.Should().Be(1);
        user.DomainEvents.Should().Contain(e => e is UserEvents.UserLoggedIn);
    }

    [Fact(DisplayName = "LockAccount: Should set lockout end")]
    public void LockAccount_Should_SetLockout()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var lockoutEnd = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        user.LockAccount(lockoutEnd);

        // Assert
        user.LockoutEnabled.Should().BeTrue();
        user.LockoutEnd.Should().Be(lockoutEnd);
        user.DomainEvents.Should().Contain(e => e is UserEvents.AccountLocked);
    }

    [Fact(DisplayName = "UnlockAccount: Should clear lockout")]
    public void UnlockAccount_Should_ClearLockout()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        user.LockAccount();

        // Act
        user.UnlockAccount();

        // Assert
        user.LockoutEnd.Should().BeNull();
        user.AccessFailedCount.Should().Be(0);
        user.DomainEvents.Should().Contain(e => e is UserEvents.AccountUnlocked);
    }

    [Fact(DisplayName = "AssignRole: Should add role and raise event")]
    public void AssignRole_Should_AddRole()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var roleId = Guid.NewGuid().ToString();

        // Act
        var result = user.AssignRole(roleId, "Administrator");

        // Assert
        result.IsError.Should().BeFalse();
        user.UserRoles.Should().ContainSingle(ur => ur.RoleId == roleId);
        user.DomainEvents.Should().Contain(e => e is UserEvents.RoleAssigned);
    }

    [Fact(DisplayName = "AssignRole: Should fail if role already assigned")]
    public void AssignRole_ShouldFail_IfAlreadyAssigned()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var roleId = Guid.NewGuid().ToString();
        user.AssignRole(roleId, "Administrator");

        // Act
        var result = user.AssignRole(roleId, "Administrator");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("User.RoleAlreadyAssigned");
    }

    [Fact(DisplayName = "Delete: Should fail if user has active refresh tokens")]
    public void Delete_ShouldFail_IfHasActiveTokens()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var token = RefreshToken.Create(user, "token", TimeSpan.FromDays(1), "127.0.0.1").Value;
        user.RefreshTokens.Add(token);

        // Act
        var result = user.Delete();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.HasActiveTokens);
    }

    [Fact(DisplayName = "SetDefaultAddress: Should mark only one address as default")]
    public void SetDefaultAddress_Should_WorkCorrectly()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var addressVal = Address.Create("Street", "City", "12345", "US").Value;
        
        var addr1 = UserAddress.Create(user.Id, addressVal, "Home").Value;
        var addr2 = UserAddress.Create(user.Id, addressVal, "Work").Value;
        user.AddAddress(addr1);
        user.AddAddress(addr2);

        // Act
        user.SetDefaultAddress(addr1.Id);

        // Assert
        addr1.IsDefault.Should().BeTrue();
        addr2.IsDefault.Should().BeFalse();

        // Act: Switch default
        user.SetDefaultAddress(addr2.Id);

        // Assert
        addr1.IsDefault.Should().BeFalse();
        addr2.IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "EnsureCustomerProfile: Should be idempotent")]
    public void EnsureCustomerProfile_ShouldBeIdempotent()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;

        // Act
        user.EnsureCustomerProfile();
        var profile1 = user.CustomerProfile;
        user.EnsureCustomerProfile();
        var profile2 = user.CustomerProfile;

        // Assert
        profile1.Should().NotBeNull();
        profile2.Should().BeSameAs(profile1);
    }

    [Fact(DisplayName = "JoinGroup: Should not add duplicate groups")]
    public void JoinGroup_Should_NotDuplicate()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var groupId = Guid.NewGuid();

        // Act
        user.JoinGroup(groupId);
        user.JoinGroup(groupId);

        // Assert
        user.GroupMemberships.Should().HaveCount(1);
    }
}