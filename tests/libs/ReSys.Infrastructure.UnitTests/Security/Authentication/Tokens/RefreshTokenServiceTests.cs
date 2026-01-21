using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ReSys.Core.Domain.Identity.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Infrastructure.Persistence;
using ReSys.Infrastructure.Security.Authentication.Tokens.Options;
using ReSys.Infrastructure.Security.Authentication.Tokens.Services;
using ReSys.Core.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;

namespace ReSys.Infrastructure.UnitTests.Security.Authentication.Tokens;

public class RefreshTokenServiceTests
{
    private readonly RefreshTokenService _sut;
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IOptions<JwtOptions> _options;

    public RefreshTokenServiceTests()
    {
        var (context, userManager, _, _) = IdentityTestFactory.CreateRealIdentityContext();
        _context = context;
        _userManager = userManager;
        _options = Options.Create(new JwtOptions
        {
            RefreshTokenLifetimeDays = 7,
            RefreshTokenRememberMeLifetimeDays = 30
        });
        _sut = new RefreshTokenService((IApplicationDbContext)_context, _userManager, _options);
    }

    [Fact(DisplayName = "GenerateRefreshTokenAsync: Should create and store a refresh token")]
    public async Task GenerateRefreshTokenAsync_Should_CreateToken()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser").Value;
        await _userManager.CreateAsync(user);
        var ip = "127.0.0.1";

        // Act
        var result = await _sut.GenerateRefreshTokenAsync(user.Id, ip, false, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Token.Should().NotBeNullOrEmpty();
        
        var storedToken = await _context.Set<RefreshToken>().FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        storedToken.Should().NotBeNull();
        storedToken!.UserId.Should().Be(user.Id);
        storedToken.TokenHash.Should().Be(RefreshToken.Hash(result.Value.Token));
    }

    [Fact(DisplayName = "RotateRefreshTokenAsync: Should revoke old token and create a new one")]
    public async Task RotateRefreshTokenAsync_Should_RotateTokens()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser").Value;
        await _userManager.CreateAsync(user);
        var ip = "127.0.0.1";
        var initialTokenResult = await _sut.GenerateRefreshTokenAsync(user.Id, ip, false, TestContext.Current.CancellationToken);
        var rawInitialToken = initialTokenResult.Value.Token;

        // Act
        var rotateResult = await _sut.RotateRefreshTokenAsync(rawInitialToken, ip, false, TestContext.Current.CancellationToken);

        // Assert
        rotateResult.IsError.Should().BeFalse();
        rotateResult.Value.Token.Should().NotBeNullOrEmpty();
        rotateResult.Value.Token.Should().NotBe(rawInitialToken);

        var tokens = await _context.Set<RefreshToken>().ToListAsync(TestContext.Current.CancellationToken);
        tokens.Should().HaveCount(2);
        
        var oldToken = tokens.FirstOrDefault(t => t.TokenHash == RefreshToken.Hash(rawInitialToken));
        oldToken.Should().NotBeNull();
        oldToken!.IsRevoked.Should().BeTrue();
        oldToken.RevokedByIp.Should().Be(ip);

        var newToken = tokens.First(t => t.TokenHash == RefreshToken.Hash(rotateResult.Value.Token));
        newToken.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "RevokeTokenAsync: Should mark token as revoked")]
    public async Task RevokeTokenAsync_Should_RevokeToken()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser").Value;
        await _userManager.CreateAsync(user);
        var ip = "127.0.0.1";
        var tokenResult = await _sut.GenerateRefreshTokenAsync(user.Id, ip, false, TestContext.Current.CancellationToken);

        // Act
        await _sut.RevokeTokenAsync(tokenResult.Value.Token, ip, "Manual Revocation", TestContext.Current.CancellationToken);

        // Assert
        var storedToken = await _context.Set<RefreshToken>().FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        storedToken.Should().NotBeNull();
        storedToken!.IsRevoked.Should().BeTrue();
        storedToken.RevokedReason.Should().Be("Manual Revocation");
    }

    [Fact(DisplayName = "RotateRefreshTokenAsync: Reuse of revoked token should revoke entire family")]
    public async Task RotateRefreshTokenAsync_ReuseRevoked_ShouldRevokeFamily()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser").Value;
        await _userManager.CreateAsync(user);
        var ip = "127.0.0.1";
        
        var t1Result = await _sut.GenerateRefreshTokenAsync(user.Id, ip, false, TestContext.Current.CancellationToken);
        var rawT1 = t1Result.Value.Token;
        
        // Rotate once: rawT1 is now revoked
        var t2Result = await _sut.RotateRefreshTokenAsync(rawT1, ip, false, TestContext.Current.CancellationToken);
        var rawT2 = t2Result.Value.Token;
        
        // Act: Reuse rawT1 (reuse detected -> family revocation)
        var reuseResult = await _sut.RotateRefreshTokenAsync(rawT1, ip, false, TestContext.Current.CancellationToken);

        // Assert
        reuseResult.IsError.Should().BeTrue();
        reuseResult.FirstError.Code.Should().Be(RefreshTokenErrors.Revoked.Code);

        // All tokens in the family should be revoked
        var allTokens = await _context.Set<RefreshToken>().ToListAsync(TestContext.Current.CancellationToken);
        allTokens.Should().NotBeEmpty();
        allTokens.Should().OnlyContain(t => t.RevokedAt != null);
    }
}
