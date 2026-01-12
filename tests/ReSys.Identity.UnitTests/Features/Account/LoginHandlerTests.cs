using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ReSys.Identity.Domain;
using ReSys.Identity.Features.Account;
using Xunit;

namespace ReSys.Identity.UnitTests.Features.Account;

public class LoginHandlerTests
{
    private UserManager<ApplicationUser> CreateMockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>, IUserEmailStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var handler = new Login.Handler(userManager);
        var request = new Login.Request("test@test.com", "Password123!", false);
        var command = new Login.Command(request);
        var user = new ApplicationUser { Email = request.Email, UserName = request.Email };

        userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
        userManager.IsLockedOutAsync(Arg.Any<ApplicationUser>()).Returns(false);
        userManager.CheckPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnValidationError()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var handler = new Login.Handler(userManager);
        var request = new Login.Request("test@test.com", "WrongPassword", false);
        var command = new Login.Command(request);
        var user = new ApplicationUser { Email = request.Email, UserName = request.Email };

        userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
        userManager.IsLockedOutAsync(Arg.Any<ApplicationUser>()).Returns(false);
        userManager.CheckPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WhenLockedOut_ShouldReturnLockedOutError()
    {
        // Arrange
        var userManager = CreateMockUserManager();
        var handler = new Login.Handler(userManager);
        var request = new Login.Request("test@test.com", "Password123!", false);
        var command = new Login.Command(request);
        var user = new ApplicationUser { Email = request.Email, UserName = request.Email };

        userManager.FindByEmailAsync(Arg.Any<string>()).Returns(user);
        userManager.IsLockedOutAsync(Arg.Any<ApplicationUser>()).Returns(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.LockedOut");
    }
}