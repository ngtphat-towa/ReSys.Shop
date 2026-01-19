using ReSys.Core.Domain.Identity.Tokens;
using ReSys.Core.Domain.Identity.Users;

namespace ReSys.Core.UnitTests.Domain.Identity.Tokens;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "RefreshToken")]
public class RefreshTokenTests
{
    private readonly User _testUser = User.Create("test@example.com").Value;

    [Fact(DisplayName = "Create: Should successfully initialize refresh token")]
    public void Create_Should_InitializeToken()
    {
        // Act
        var result = RefreshToken.Create(_testUser, "raw-token", TimeSpan.FromDays(7), "127.0.0.1");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UserId.Should().Be(_testUser.Id);
        result.Value.CreatedByIp.Should().Be("127.0.0.1");
        result.Value.IsActive.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is RefreshTokenEvents.TokenGenerated);
    }

    [Fact(DisplayName = "IsExpired: Should correctly detect expiration")]
    public void IsExpired_Should_DetectExpiration()
    {
        // Act
        var token = RefreshToken.Create(_testUser, "token", TimeSpan.FromSeconds(-1), "127.0.0.1").Value;

        // Assert
        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Revoke: Should mark as revoked")]
    public void Revoke_Should_MarkAsRevoked()
    {
        // Arrange
        var token = RefreshToken.Create(_testUser, "token", TimeSpan.FromDays(1), "127.0.0.1").Value;

        // Act
        var result = token.Revoke("192.168.1.1", "User logged out");

        // Assert
        result.IsError.Should().BeFalse();
        token.IsRevoked.Should().BeTrue();
        token.RevokedByIp.Should().Be("192.168.1.1");
        token.RevokedReason.Should().Be("User logged out");
        token.IsActive.Should().BeFalse();
        token.DomainEvents.Should().Contain(e => e is RefreshTokenEvents.TokenRevoked);
    }

    [Fact(DisplayName = "GenerateRandomToken: Should return non-empty string")]
    public void GenerateRandomToken_Should_ReturnString()
    {
        // Act
        var token = RefreshToken.GenerateRandomToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }
}