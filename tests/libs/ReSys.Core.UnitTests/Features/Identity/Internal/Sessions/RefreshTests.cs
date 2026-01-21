using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Sessions;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;
using Mapster;
using ReSys.Core.Features.Shared.Identity.Internal.Common;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Sessions;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Sessions")]
public class RefreshTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();

    static RefreshTests()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<AuthenticationResult, Refresh.Response>();
    }

    [Fact(DisplayName = "Handle: Should successfully rotate and refresh tokens")]
    public async Task Handle_ValidToken_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("refresh@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user);

        var oldToken = "old-refresh-token";
        var newToken = "new-refresh-token";
        var accessToken = "new-access-token";
        var ip = "127.0.0.1";

        _refreshTokenService.RotateRefreshTokenAsync(oldToken, ip, false, Arg.Any<CancellationToken>())
            .Returns(new TokenResult(newToken, 1000));

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) }));
        _jwtTokenService.GetPrincipalFromToken(newToken).Returns(claims);

        _jwtTokenService.GenerateAccessTokenAsync(user, Arg.Any<CancellationToken>())
            .Returns(new TokenResult(accessToken, 500));

        var handler = new Refresh.Handler(userManager, _refreshTokenService, _jwtTokenService);

        // Act
        var result = await handler.Handle(new Refresh.Command(new Refresh.Request(oldToken, false, ip)), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be(newToken);
    }

    [Fact(DisplayName = "Handle: Should fail and revoke if user is inactive")]
    public async Task Handle_InactiveUser_ShouldRevokeAndFail()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("inactive@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        user.UpdateStatus(false); // Inactive
        await userManager.CreateAsync(user);

        var token = "some-token";
        var ip = "127.0.0.1";

        _refreshTokenService.RotateRefreshTokenAsync(token, ip, false, Arg.Any<CancellationToken>())
            .Returns(new TokenResult(token, 1000));

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) }));
        _jwtTokenService.GetPrincipalFromToken(token).Returns(claims);

        var handler = new Refresh.Handler(userManager, _refreshTokenService, _jwtTokenService);

        // Act
        var result = await handler.Handle(new Refresh.Command(new Refresh.Request(token, false, ip)), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.AccountLocked");
        await _refreshTokenService.Received(1).RevokeTokenAsync(token, ip, "Inactive User", Arg.Any<CancellationToken>());
    }
}
