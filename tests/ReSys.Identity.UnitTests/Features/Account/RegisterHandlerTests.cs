using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ReSys.Identity.Domain;
using ReSys.Identity.Features.Account;
using Xunit;

namespace ReSys.Identity.UnitTests.Features.Account;

public class RegisterHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Register.Handler _handler;

    public RegisterHandlerTests()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
        _handler = new Register.Handler(_userManager);
    }

    [Fact]
    public async Task Handle_WhenUserCreatedSuccessfully_ShouldReturnSuccess()
    {
        // Arrange
        var request = new Register.Request("test@test.com", "Pass123$", "Pass123$");
        var command = new Register.Command(request);

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), request.Password)
            .Returns(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_ShouldReturnValidationErrors()
    {
        // Arrange
        var request = new Register.Request("test@test.com", "Pass123$", "Pass123$");
        var command = new Register.Command(request);
        var identityError = new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" };

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), request.Password)
            .Returns(IdentityResult.Failed(identityError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.FirstError.Code.Should().Be("DuplicateEmail");
        result.FirstError.Description.Should().Be("Email already exists");
    }
}
