using FluentAssertions;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Services;
using Xunit;

namespace ReSys.Infrastructure.UnitTests.Notifications.Services;

public class EmptySenderServiceTests
{
    [Fact(DisplayName = "EmptyEmailSenderService should return Disabled error")]
    public async Task EmptyEmailSender_ShouldReturnError()
    {
        // Arrange
        var service = new EmptyEmailSenderService();
        var to = NotificationRecipient.Create("test@example.com");
        var content = NotificationContent.Create("S", "B");
        var metadata = NotificationMetadata.Default;

        // Act
        var result = await service.SendAsync(to, content, metadata, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("EmailSender.Disabled");
    }

    [Fact(DisplayName = "EmptySmsSenderService should return Unavailable error")]
    public async Task EmptySmsSender_ShouldReturnError()
    {
        // Arrange
        var service = new EmptySmsSenderService();
        var to = NotificationRecipient.Create("+123");
        var content = NotificationContent.Create("", "B");
        var metadata = NotificationMetadata.Default;

        // Act
        var result = await service.SendAsync(to, content, metadata, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("SmsNotification.Unavailable");
    }
}