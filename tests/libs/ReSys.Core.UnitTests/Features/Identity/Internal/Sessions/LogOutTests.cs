using FluentAssertions;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Features.Shared.Identity.Internal.Sessions;
using ErrorOr;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Sessions;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Sessions")]
public class LogOutTests
{
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly LogOut.Handler _sut;

    public LogOutTests()
    {
        _sut = new LogOut.Handler(_refreshTokenService, _userContext);
    }

    [Fact(DisplayName = "Handle: Should successfully logout single device when token provided")]
    public async Task Handle_SingleDevice_ShouldRevokeToken()
    {
        // Arrange
        var token = "valid-refresh-token";
        var request = new LogOut.Request(token, LogOut.Mode.CurrentDevice, "127.0.0.1");
        
        _refreshTokenService.RevokeTokenAsync(token, "127.0.0.1", "User Logout", Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        var result = await _sut.Handle(new LogOut.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        await _refreshTokenService.Received(1).RevokeTokenAsync(token, "127.0.0.1", "User Logout", Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handle: Should successfully logout all devices when authenticated")]
    public async Task Handle_AllDevices_ShouldRevokeAllTokens()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = "some-token";
        var request = new LogOut.Request(token, LogOut.Mode.AllDevices, "127.0.0.1");

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(userId);
        
        _refreshTokenService.RevokeTokenAsync(token, "127.0.0.1", "Logout All", Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        var result = await _sut.Handle(new LogOut.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        await _refreshTokenService.Received(1).RevokeTokenAsync(token, "127.0.0.1", "Logout All", Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handle: Should return Unauthorized for AllDevices if not authenticated")]
    public async Task Handle_AllDevicesUnauthenticated_ShouldReturnError()
    {
        // Arrange
        var request = new LogOut.Request("token", LogOut.Mode.AllDevices);
        _userContext.IsAuthenticated.Returns(false);

        // Act
        var result = await _sut.Handle(new LogOut.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("User.Unauthorized");
    }
}
