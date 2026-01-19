using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Internal.Login;
using ReSys.Core.UnitTests.Helpers;
using ErrorOr;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Login;

public class LoginTests
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ReSys.Core.Features.Identity.Internal.Login.Login.Handler _handler;

    public LoginTests()
    {
        _userManager = IdentityTestFactory.CreateUserManager();
        _signInManager = IdentityTestFactory.CreateSignInManager(_userManager);
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _refreshTokenService = Substitute.For<IRefreshTokenService>();
        _handler = new ReSys.Core.Features.Identity.Internal.Login.Login.Handler(_userManager, _signInManager, _jwtTokenService, _refreshTokenService);
    }

    [Fact]
    public async Task Handle_WhenValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        var request = new ReSys.Core.Features.Identity.Internal.Login.Login.Request("test@example.com", "Password123!");
        var command = new ReSys.Core.Features.Identity.Internal.Login.Login.Command(request);

        _userManager.FindByEmailAsync(request.Credential).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true).Returns(SignInResult.Success);
        
        _jwtTokenService.GenerateAccessTokenAsync(user, Arg.Any<CancellationToken>())
            .Returns(new TokenResult("access-token", 12345));
        
        _refreshTokenService.GenerateRefreshTokenAsync(user.Id, Arg.Any<string>(), false, Arg.Any<CancellationToken>())
            .Returns(new TokenResult("refresh-token", 67890));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_WhenAccountLocked_ShouldReturnAccountLockedError()
    {
        // Arrange
        var user = User.Create("test@example.com").Value;
        var request = new ReSys.Core.Features.Identity.Internal.Login.Login.Request("test@example.com", "Password123!");
        var command = new ReSys.Core.Features.Identity.Internal.Login.Login.Command(request);

        _userManager.FindByEmailAsync(request.Credential).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true).Returns(SignInResult.LockedOut);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.AccountLocked");
    }
}