using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Infrastructure.Security.Authentication.Tokens.Options;
using ReSys.Infrastructure.Security.Authentication.Tokens.Services;

namespace ReSys.Infrastructure.UnitTests.Security.Authentication.Tokens;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;
    private readonly IOptions<JwtOptions> _options;
    private const string Secret = "super-secret-key-that-is-at-least-32-characters-long";

    public JwtTokenServiceTests()
    {
        _options = Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Secret = Secret,
            AccessTokenLifetimeMinutes = 15
        });
        _sut = new JwtTokenService(_options);
    }

    [Fact(DisplayName = "GenerateAccessTokenAsync: Should generate a valid JWT token")]
    public async Task GenerateAccessTokenAsync_Should_GenerateValidToken()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser").Value;

        // Act
        var result = await _sut.GenerateAccessTokenAsync(user, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.ExpiresAt.Should().BeGreaterThan(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Value.Token);

        jwtToken.Issuer.Should().Be(_options.Value.Issuer);
        jwtToken.Audiences.Should().Contain(_options.Value.Audience);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == user.UserName);
    }

    [Fact(DisplayName = "GetPrincipalFromToken: Should extract principal from valid token")]
    public async Task GetPrincipalFromToken_Should_ExtractPrincipal()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser").Value;
        var tokenResult = await _sut.GenerateAccessTokenAsync(user, TestContext.Current.CancellationToken);

        // Act
        var principalResult = _sut.GetPrincipalFromToken(tokenResult.Value.Token);

        // Assert
        principalResult.IsError.Should().BeFalse();
        principalResult.Value.Identity.Should().NotBeNull();
        principalResult.Value.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
    }

    [Theory(DisplayName = "ValidateTokenFormat: Should correctly validate token format")]
    [InlineData("valid.token.format", true)]
    [InlineData("invalidformat", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateTokenFormat_Should_ValidateCorrectly(string? token, bool expected)
    {
        // Act
        var result = _sut.ValidateTokenFormat(token!);

        // Assert
        result.Value.Should().Be(expected);
    }
}
