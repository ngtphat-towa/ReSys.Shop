using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Login;
using ReSys.Core.Features.Shared.Identity.Internal.Common;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;
using Mapster;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Auth;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Auth")]
public class LoginTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();

    static LoginTests()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<AuthenticationResult, Login.Response>();
    }

    [Fact(DisplayName = "Handle: Should successfully log in when valid")]
    public async Task Handle_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        var signInManager = sp.GetRequiredService<SignInManager<User>>();
        
        var user = User.Create("test@example.com", "testuser").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user, "Password123!");

        var request = new Login.Request("test@example.com", "Password123!", IpAddress: "127.0.0.1");
        var handler = new Login.Handler(userManager, signInManager, _jwtTokenService, _refreshTokenService);

        _jwtTokenService.GenerateAccessTokenAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(new TokenResult("access", 1));
        _refreshTokenService.GenerateRefreshTokenAsync(user.Id, "127.0.0.1", false, Arg.Any<CancellationToken>())
            .Returns(new TokenResult("refresh", 1));

        // Act
        var result = await handler.Handle(new Login.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
        
        // Verify sign-in recorded
        user.SignInCount.Should().Be(1);
        user.CurrentSignInIp.Should().Be("127.0.0.1");
    }

    [Fact(DisplayName = "Handle: Should fail when user not found")]
    public async Task Handle_UserNotFound_ShouldReturnError()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        var signInManager = sp.GetRequiredService<SignInManager<User>>();

        var request = new Login.Request("nonexistent@example.com", "Password123!");
        var handler = new Login.Handler(userManager, signInManager, _jwtTokenService, _refreshTokenService);

        // Act
        var result = await handler.Handle(new Login.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("User.NotFound");
    }

    [Fact(DisplayName = "Handle: Should fail when password is incorrect")]
    public async Task Handle_WrongPassword_ShouldReturnError()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        var signInManager = sp.GetRequiredService<SignInManager<User>>();
        
        var user = User.Create("test@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user, "CorrectPassword123!");

        var request = new Login.Request("test@example.com", "WrongPassword");
        var handler = new Login.Handler(userManager, signInManager, _jwtTokenService, _refreshTokenService);

        // Act
        var result = await handler.Handle(new Login.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact(DisplayName = "Handle: Should fail when account is locked")]
    public async Task Handle_LockedAccount_ShouldReturnError()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        var signInManager = sp.GetRequiredService<SignInManager<User>>();
        
        var user = User.Create("locked@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user, "Password123!");
        
        // Lock user
        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddHours(1));

        var request = new Login.Request("locked@example.com", "Password123!");
        var handler = new Login.Handler(userManager, signInManager, _jwtTokenService, _refreshTokenService);

        // Act
        var result = await handler.Handle(new Login.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.AccountLocked");
    }
}