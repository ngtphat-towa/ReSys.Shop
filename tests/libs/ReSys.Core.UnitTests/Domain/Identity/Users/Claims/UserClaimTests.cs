using ReSys.Core.Domain.Identity.Users.Claims;

namespace ReSys.Core.UnitTests.Domain.Identity.Users.Claims;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "UserClaim")]
public class UserClaimTests
{
    private readonly string _userId = Guid.NewGuid().ToString();

    [Fact(DisplayName = "Create: Should successfully initialize user claim")]
    public void Create_Should_InitializeUserClaim()
    {
        // Act
        var result = UserClaim.Create(_userId, "Permission", "Read", "Admin");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UserId.Should().Be(_userId);
        result.Value.ClaimType.Should().Be("Permission");
        result.Value.ClaimValue.Should().Be("Read");
        result.Value.AssignedBy.Should().Be("Admin");
        result.Value.AssignedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Create: Should fail if claim type is missing")]
    public void Create_ShouldFail_IfClaimTypeMissing()
    {
        // Act
        var result = UserClaim.Create(_userId, "");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserClaimErrors.ClaimTypeRequired);
    }

    [Fact(DisplayName = "Create: Should fail if claim value too long")]
    public void Create_ShouldFail_IfValueTooLong()
    {
        // Arrange
        var longValue = new string('A', UserClaimConstraints.ClaimValueMaxLength + 1);

        // Act
        var result = UserClaim.Create(_userId, "Type", longValue);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserClaimErrors.ClaimValueTooLong(UserClaimConstraints.ClaimValueMaxLength));
    }
}