using FluentAssertions;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Core.Common.Notifications.Errors;
using ReSys.Core.Common.Notifications.Builders;
using Xunit;

namespace ReSys.Core.UnitTests.Common.Notifications.Models;

public class NotificationMapperTests
{
    [Fact(DisplayName = "NotificationMapper should correctly map content and replace placeholders")]
    public void MapContent_ShouldReplacePlaceholders()
    {
        // Arrange
        var context = NotificationContext.Create(
            (NotificationConstants.Parameter.UserFirstName, "Alice"),
            (NotificationConstants.Parameter.OrderId, "ORD-123")
        );
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            NotificationRecipient.Create("alice@example.com"))
            .WithContext(context)
            .Value;

        // Act
        var result = message.MapContent();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Subject.Should().Contain("Order Confirmation");
        result.Value.Body.Should().Contain("Alice");
        result.Value.Body.Should().Contain("ORD-123");
    }

    [Fact(DisplayName = "NotificationMapper should replace all occurrences of the same placeholder")]
    public void MapContent_WithDuplicatePlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var context = NotificationContext.Create(
            (NotificationConstants.Parameter.UserFirstName, "Alice")
        );
        
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemActiveEmail,
            NotificationRecipient.Create("alice@example.com"))
            .WithContext(NotificationContext.ApplyParameter(context, NotificationConstants.Parameter.SystemName, "MyStore"))
            .Value;

        // Act
        var result = message.MapContent();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.HtmlBody.Should().Contain("MyStore Logo");
        result.Value.HtmlBody.Should().Contain("Welcome to MyStore");
    }

    [Fact(DisplayName = "NotificationMapper should handle missing parameters by keeping placeholders")]
    public void MapContent_WhenParametersMissing_ShouldKeepPlaceholders()
    {
        // Arrange
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            NotificationRecipient.Create("alice@example.com")).Value;

        // Act
        var result = message.MapContent();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Body.Should().Contain("{UserFirstName}");
        result.Value.Body.Should().Contain("{OrderId}");
    }

    [Fact(DisplayName = "NotificationMapper should correctly handle extra parameters in context")]
    public void MapContent_WithExtraParameters_ShouldStillWork()
    {
        // Arrange
        var context = NotificationContext.Create(
            (NotificationConstants.Parameter.UserFirstName, "Alice"),
            (NotificationConstants.Parameter.OrderId, "ORD-123"),
            (NotificationConstants.Parameter.PromoCode, "EXTRA-PARAM") // Extra param not in template
        );
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            NotificationRecipient.Create("alice@example.com"))
            .WithContext(context)
            .Value;

        // Act
        var result = message.MapContent();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Body.Should().Contain("Alice");
        result.Value.Body.Should().Contain("ORD-123");
    }

    [Fact(DisplayName = "NotificationMapper should correctly map to Sms UseCase and replace placeholders")]
    public void MapContent_Sms_ShouldReplacePlaceholders()
    {
        // Arrange
        var context = NotificationContext.Create(
            (NotificationConstants.Parameter.UserFirstName, "Bob"),
            (NotificationConstants.Parameter.OtpCode, "999888")
        );
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.System2FaOtp,
            NotificationRecipient.Create("+123456789"))
            .WithContext(context)
            .Value;

        // Act
        var result = message.MapContent();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Body.Should().Contain("Bob");
        result.Value.Body.Should().Contain("999888");
    }

    [Fact(DisplayName = "NotificationMapper should return error when UseCase template is missing")]
    public void MapContent_WhenUseCaseMissing_ShouldReturnError()
    {
        // Arrange
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.None,
            NotificationRecipient.Create("test@example.com")).Value;

        // Act
        var result = message.MapContent();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(NotificationErrors.General.TemplateNotFound(NotificationConstants.UseCase.None.ToString()).Code);
    }
}
