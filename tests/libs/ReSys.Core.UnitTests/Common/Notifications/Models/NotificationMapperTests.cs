using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Core.Common.Notifications.Errors;
using ReSys.Core.Common.Notifications.Builders;

namespace ReSys.Core.UnitTests.Common.Notifications.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Notifications")]
public class NotificationMapperTests
{
    [Fact(DisplayName = "MapContent: Should correctly map content and replace placeholders")]
    public void MapContent_Should_ReplacePlaceholders()
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

    [Fact(DisplayName = "MapContent: Should replace all occurrences of the same placeholder")]
    public void MapContent_Should_ReplaceAll_WhenDuplicatePlaceholdersExist()
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

    [Fact(DisplayName = "MapContent: Should keep placeholders when parameters are missing")]
    public void MapContent_Should_KeepPlaceholders_WhenParametersMissing()
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

    [Fact(DisplayName = "MapContent: Should correctly handle extra parameters in context")]
    public void MapContent_Should_StillWork_WithExtraParameters()
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

    [Fact(DisplayName = "MapContent: Should correctly map to Sms UseCase and replace placeholders")]
    public void MapContent_Should_ReplacePlaceholders_ForSms()
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

    [Fact(DisplayName = "MapContent: Should return error when UseCase template is missing")]
    public void MapContent_Should_ReturnError_WhenUseCaseTemplateMissing()
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
