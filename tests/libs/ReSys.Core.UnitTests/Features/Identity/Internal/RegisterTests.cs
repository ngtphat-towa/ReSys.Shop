using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;


using NSubstitute;


using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.UnitTests.Helpers;


using ErrorOr;
using ReSys.Core.Features.Shared.Identity.Internal.Register;

namespace ReSys.Core.UnitTests.Features.Identity.Internal;

public class RegisterTests
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly Register.Handler _handler;

    public RegisterTests()
    {
        _userManager = IdentityTestFactory.CreateUserManager();
        _roleManager = IdentityTestFactory.CreateRoleManager();
        _notificationService = Substitute.For<INotificationService>();
        _configuration = Substitute.For<IConfiguration>();
        _handler = new Register.Handler(_userManager, _roleManager, _notificationService, _configuration);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateUserAndAssignRole()
    {
        // Arrange
        var request = new Register.Request { Email = "test@example.com", Password = "Password123!", ConfirmPassword = "Password123!" };
        var command = new Register.Command(request);

        _userManager.FindByEmailAsync(request.Email).Returns((User)null!);
        _roleManager.RoleExistsAsync("Storefront.Customer").Returns(true);
        _userManager.CreateAsync(Arg.Any<User>(), request.Password).Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<User>(), "Storefront.Customer").Returns(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _userManager.Received(1).CreateAsync(Arg.Any<User>(), request.Password);
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<User>(), "Storefront.Customer");
    }

    [Fact]
    public async Task Handle_WhenNotificationFails_ShouldRollbackUserCreation()
    {
        // Arrange
        var request = new Register.Request { Email = "test@example.com", Password = "Password123!", ConfirmPassword = "Password123!" };
        var command = new Register.Command(request);

        _userManager.FindByEmailAsync(request.Email).Returns((User)null!);
        _roleManager.RoleExistsAsync("Storefront.Customer").Returns(true);
        _userManager.CreateAsync(Arg.Any<User>(), request.Password).Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<User>(), "Storefront.Customer").Returns(IdentityResult.Success);

        // Force notification to fail
        _notificationService.SendAsync(Arg.Any<Core.Common.Notifications.Models.NotificationMessage>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Notification.Error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Register.AtomicFailure");
        await _userManager.Received(1).DeleteAsync(Arg.Any<User>());
    }
}