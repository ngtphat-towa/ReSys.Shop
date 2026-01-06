using ErrorOr;
using FluentAssertions;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Options;
using ReSys.Infrastructure.Notifications.Services;
using ReSys.Core.Common.Notifications.Errors;
using Xunit;

namespace ReSys.Infrastructure.UnitTests.Notifications.Services;

public class EmailSenderServiceTests
{
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<EmailSenderService> _logger;
    private readonly EmailSenderService _service;

    public EmailSenderServiceTests()
    {
        _fluentEmail = Substitute.For<IFluentEmail>();
        _logger = Substitute.For<ILogger<EmailSenderService>>();
        
        var options = Options.Create(new SmtpOptions
        {
            FromEmail = "test@example.com",
            FromName = "Test",
            EnableEmailNotifications = true,
            Provider = "smtp",
            SmtpConfig = new SmtpConfig { Host = "localhost", Port = 25 }
        });

        _service = new EmailSenderService(options, _fluentEmail, _logger);
    }

    [Fact(DisplayName = "Email Sender should return success when email is sent successfully")]
    public async Task SendAsync_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var to = NotificationRecipient.Create("test@example.com");
        var content = NotificationContent.Create("Subject", "Body");
        var metadata = NotificationMetadata.Default;
        
        _fluentEmail.SetFrom(Arg.Any<string>(), Arg.Any<string>()).Returns(_fluentEmail);
        _fluentEmail.To(Arg.Any<string>(), Arg.Any<string>()).Returns(_fluentEmail);
        _fluentEmail.Subject(Arg.Any<string>()).Returns(_fluentEmail);
        _fluentEmail.PlaintextAlternativeBody(Arg.Any<string>()).Returns(_fluentEmail);
        _fluentEmail.Body(Arg.Any<string>(), Arg.Any<bool>()).Returns(_fluentEmail);
        _fluentEmail.SendAsync(Arg.Any<CancellationToken>()).Returns(new SendResponse { MessageId = "1" });

        // Act
        var result = await _service.SendAsync(to, content, metadata, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact(DisplayName = "Email Sender should return failure when FluentEmail fails to send")]
    public async Task SendAsync_WhenFluentEmailFails_ShouldReturnFailure()
    {
        // Arrange
        var to = NotificationRecipient.Create("test@example.com");
        var content = NotificationContent.Create("Subject", "Body");
        var metadata = NotificationMetadata.Default;
        
        _fluentEmail.SetFrom(Arg.Any<string>(), Arg.Any<string>()).Returns(_fluentEmail);
        _fluentEmail.To(Arg.Any<string>(), Arg.Any<string>()).Returns(_fluentEmail);
        _fluentEmail.Subject(Arg.Any<string>()).Returns(_fluentEmail);
        _fluentEmail.PlaintextAlternativeBody(Arg.Any<string>()).Returns(_fluentEmail);
        _fluentEmail.Body(Arg.Any<string>(), Arg.Any<bool>()).Returns(_fluentEmail);
        _fluentEmail.SendAsync(Arg.Any<CancellationToken>()).Returns(new SendResponse { ErrorMessages = ["Error detail"] });

        // Act
        var result = await _service.SendAsync(to, content, metadata, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(NotificationErrors.Email.SendFailed("").Code);
    }
}