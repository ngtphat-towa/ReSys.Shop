using ErrorOr;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Services;
using Xunit;

namespace ReSys.Infrastructure.UnitTests.Notifications;

public class NotificationServiceTests
{
    private readonly IEmailSenderService _emailSenderService;
    private readonly ISmsSenderService _smsSenderService;
    private readonly IValidator<NotificationData> _validator;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _emailSenderService = Substitute.For<IEmailSenderService>();
        _smsSenderService = Substitute.For<ISmsSenderService>();
        _validator = Substitute.For<IValidator<NotificationData>>();
        _notificationService = new NotificationService(_emailSenderService, _smsSenderService, _validator);
    }

    [Fact]
    public async Task AddNotificationAsync_WhenValidationFails_ShouldReturnErrors()
    {
        // Arrange
        var notificationData = new NotificationData
        {
            UseCase = NotificationConstants.UseCase.SystemOrderConfirmation,
            Receivers = ["test@example.com"],
            CreatedBy = "Tester"
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("UseCase", "Invalid use case") { ErrorCode = "Notification.UseCase.Invalid" }
        };

        _validator.ValidateAsync(notificationData, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        // Act
        var result = await _notificationService.AddNotificationAsync(notificationData, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Notification.UseCase.Invalid");
        await _emailSenderService.DidNotReceive().AddEmailNotificationAsync(Arg.Any<EmailNotificationData>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddNotificationAsync_WhenMethodIsEmail_ShouldCallEmailSender()
    {
        // Arrange
        var notificationData = new NotificationData
        {
            UseCase = NotificationConstants.UseCase.SystemOrderConfirmation,
            SendMethodType = NotificationConstants.SendMethod.Email,
            Receivers = ["test@example.com"],
            Title = "Test Email",
            Content = "Test Content",
            CreatedBy = "Tester"
        };

        _validator.ValidateAsync(notificationData, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _emailSenderService.AddEmailNotificationAsync(Arg.Any<EmailNotificationData>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        var result = await _notificationService.AddNotificationAsync(notificationData, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        await _emailSenderService.Received(1).AddEmailNotificationAsync(
            Arg.Is<EmailNotificationData>(e => e.Title == "Test Email"), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddNotificationAsync_WhenMethodIsSms_ShouldCallSmsSender()
    {
        // Arrange
        var notificationData = new NotificationData
        {
            UseCase = NotificationConstants.UseCase.System2FaOtp,
            SendMethodType = NotificationConstants.SendMethod.SMS,
            Receivers = ["+1234567890"],
            Content = "Your OTP is 1234",
            CreatedBy = "Tester"
        };

        _validator.ValidateAsync(notificationData, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _smsSenderService.AddSmsNotificationAsync(Arg.Any<SmsNotificationData>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        var result = await _notificationService.AddNotificationAsync(notificationData, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        await _smsSenderService.Received(1).AddSmsNotificationAsync(
            Arg.Is<SmsNotificationData>(s => s.Content == "Your OTP is 1234"), 
            Arg.Any<CancellationToken>());
    }
}
