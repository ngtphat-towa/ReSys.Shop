using ErrorOr;

using NSubstitute;

using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Services;
using ReSys.Core.Common.Notifications.Errors;
using ReSys.Core.Common.Notifications.Validators;
using ReSys.Core.Common.Notifications.Builders;

using FluentValidation;

namespace ReSys.Infrastructure.UnitTests.Notifications.Services;

public class NotificationServiceTests
{
    private readonly IEmailSenderService _emailSenderService;
    private readonly ISmsSenderService _smsSenderService;
    private readonly IValidator<NotificationMessage> _validator;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _emailSenderService = Substitute.For<IEmailSenderService>();
        _smsSenderService = Substitute.For<ISmsSenderService>();
        _validator = new NotificationMessageValidator();
        _notificationService = new NotificationService(_emailSenderService, _smsSenderService, _validator);
    }

    [Fact(DisplayName = "SendAsync should return validation error for invalid recipient")]
    public async Task SendAsync_WhenInvalidRecipient_ShouldReturnValidationError()
    {
        // Arrange
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            NotificationRecipient.Create("not-an-email")).Value;

        // Act
        var result = await _notificationService.SendAsync(message, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(NotificationErrors.Email.InvalidFormat.Code);
    }

    [Fact(DisplayName = "SendAsync should return TemplateNotFound for unknown UseCase")]
    public async Task SendAsync_WhenTemplateMissing_ShouldReturnError()
    {
        // Arrange
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.None,
            NotificationRecipient.Create("test@example.com")).Value;

        // Act
        var result = await _notificationService.SendAsync(message, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(NotificationErrors.General.TemplateNotFound("").Code);
    }

    [Fact(DisplayName = "SendAsync should propagate error from EmailSenderService")]
    public async Task SendAsync_WhenSenderFails_ShouldReturnSenderError()
    {
        // Arrange
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            NotificationRecipient.Create("user@example.com")).Value;

        var expectedError = Error.Failure("Email.Failed", "SMTP Down");
        _emailSenderService.SendAsync(Arg.Any<NotificationRecipient>(), Arg.Any<NotificationContent>(), Arg.Any<NotificationMetadata>(), Arg.Any<IEnumerable<NotificationAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        var result = await _notificationService.SendAsync(message, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);
    }

    [Fact(DisplayName = "SendAsync should route to Email Sender correctly")]
    public async Task SendAsync_WhenEmailUseCase_ShouldRouteToEmailSender()
    {
        // Arrange
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            NotificationRecipient.Create("user@example.com"))
            .WithUserFirstName("John")
            .Value;

        _emailSenderService.SendAsync(Arg.Any<NotificationRecipient>(), Arg.Any<NotificationContent>(), Arg.Any<NotificationMetadata>(), Arg.Any<IEnumerable<NotificationAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        var result = await _notificationService.SendAsync(message, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        await _emailSenderService.Received(1).SendAsync(
            Arg.Any<NotificationRecipient>(),
            Arg.Is<NotificationContent>(c => c.Subject.Contains("Order Confirmation")),
            Arg.Any<NotificationMetadata>(),
            Arg.Any<IEnumerable<NotificationAttachment>>(),
            Arg.Any<CancellationToken>());
    }
}