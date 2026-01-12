using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Identity.Domain;
using ReSys.Identity.Features.Account;
using Xunit;

namespace ReSys.Identity.UnitTests.Features.Account;

public class ChangeEmailHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly ChangeEmail.Handler _handler;

    public ChangeEmailHandlerTests()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
        _notificationService = Substitute.For<INotificationService>();
        _handler = new ChangeEmail.Handler(_userManager, _notificationService);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSendNotificationAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var newEmail = "new@test.com";
        var user = new ApplicationUser { Id = userId, Email = "old@test.com", UserName = "old@test.com" };
        var command = new ChangeEmail.Command(userId, new ChangeEmail.Request(newEmail));

        _userManager.FindByIdAsync(userId).Returns(user);
        _userManager.GenerateChangeEmailTokenAsync(user, newEmail).Returns("test-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _notificationService.Received(1).SendAsync(
            Arg.Is<NotificationMessage>(m => 
                m.UseCase == NotificationConstants.UseCase.SystemAccountUpdate && 
                m.Recipient.Identifier == newEmail),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailIsSame_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "same@test.com";
        var user = new ApplicationUser { Id = userId, Email = email };
        var command = new ChangeEmail.Command(userId, new ChangeEmail.Request(email));

        _userManager.FindByIdAsync(userId).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Email.Same");
    }
}
