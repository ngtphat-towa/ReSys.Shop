using FluentAssertions;
using ReSys.Core.Common.Notifications.Builders;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;
using Xunit;

namespace ReSys.Core.UnitTests.Common.Notifications.Builders;

public class NotificationMessageBuilderTests
{
    #region Initializer Tests
    [Fact(DisplayName = "ForUseCase.To should create a valid message")]
    public void ForUseCase_To_ShouldWork()
    {
        var result = NotificationMessageBuilder
            .ForUseCase(NotificationConstants.UseCase.SystemOrderConfirmation)
            .To(NotificationRecipient.Create("test@test.com"));

        result.IsError.Should().BeFalse();
        result.Value.UseCase.Should().Be(NotificationConstants.UseCase.SystemOrderConfirmation);
        result.Value.Recipient.Identifier.Should().Be("test@test.com");
    }

    [Fact(DisplayName = "Create should create a valid message")]
    public void Create_ShouldWork()
    {
        var result = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.System2FaOtp, 
            NotificationRecipient.Create("+12345"));

        result.IsError.Should().BeFalse();
        result.Value.UseCase.Should().Be(NotificationConstants.UseCase.System2FaOtp);
    }
    #endregion

    #region Message Chaining Tests
    [Fact(DisplayName = "WithMetadata should set metadata correctly")]
    public void WithMetadata_ShouldWork()
    {
        var meta = NotificationMetadata.Create(NotificationConstants.PriorityLevel.High, "cs-CZ", "Admin");
        var result = NotificationMessageBuilder
            .Create(NotificationConstants.UseCase.SystemResetPassword, NotificationRecipient.Create("a@b.com"))
            .WithMetadata(meta);

        result.Value.Metadata.Priority.Should().Be(NotificationConstants.PriorityLevel.High);
        result.Value.Metadata.Language.Should().Be("cs-CZ");
        result.Value.Metadata.CreatedBy.Should().Be("Admin");
    }

    [Fact(DisplayName = "AddAttachment should accumulate multiple attachments")]
    public void AddAttachment_ShouldWork()
    {
        var att1 = NotificationAttachment.Create("1.txt", [1], "text/plain");
        var att2 = NotificationAttachment.Create("2.txt", [2], "text/plain");

        var result = NotificationMessageBuilder
            .Create(NotificationConstants.UseCase.SystemOrderConfirmation, NotificationRecipient.Create("a@b.com"))
            .AddAttachment(att1)
            .AddAttachment(att2);

        result.Value.Attachments.Should().HaveCount(2);
        result.Value.Attachments.Should().Contain([att1, att2]);
    }

    [Fact(DisplayName = "WithContext should replace entire context")]
    public void WithContext_ShouldWork()
    {
        var context = NotificationContext.Create((NotificationConstants.Parameter.OrderId, "999"));
        var result = NotificationMessageBuilder
            .Create(NotificationConstants.UseCase.SystemOrderConfirmation, NotificationRecipient.Create("a@b.com"))
            .WithUserFirstName("Initial")
            .WithContext(context);

        result.Value.Context.GetValue(NotificationConstants.Parameter.OrderId).Should().Be("999");
        result.Value.Context.GetValue(NotificationConstants.Parameter.UserFirstName).Should().BeNull();
    }
    #endregion

    #region Context Builder Tests
    [Fact(DisplayName = "CreateContext should start empty flow")]
    public void CreateContext_ShouldWork()
    {
        var result = NotificationMessageBuilder.CreateContext()
            .AddParam(NotificationConstants.Parameter.SystemName, "ReSys");

        result.Value.GetValue(NotificationConstants.Parameter.SystemName).Should().Be("ReSys");
    }

    [Fact(DisplayName = "AddParams dictionary should batch add")]
    public void AddParams_Dictionary_ShouldWork()
    {
        var dict = new Dictionary<NotificationConstants.Parameter, string?> 
        {
            [NotificationConstants.Parameter.OrderId] = "1",
            [NotificationConstants.Parameter.PromoCode] = "SAVE"
        };

        var result = NotificationMessageBuilder.CreateContext()
            .AddParams(dict);

        result.Value.GetValue(NotificationConstants.Parameter.OrderId).Should().Be("1");
        result.Value.GetValue(NotificationConstants.Parameter.PromoCode).Should().Be("SAVE");
    }
    #endregion

    #region Semantic Helpers Tests
    [Fact(DisplayName = "Message Semantic Helpers should set correct params")]
    public void Message_SemanticHelpers_ShouldWork()
    {
        var result = NotificationMessageBuilder
            .ForUseCase(NotificationConstants.UseCase.System2FaOtp)
            .To(NotificationRecipient.Create("+1"))
            .WithUserFirstName("Bob")
            .WithOrderId("ORD-1")
            .WithOtpCode("123");

        result.Value.Context.GetValue(NotificationConstants.Parameter.UserFirstName).Should().Be("Bob");
        result.Value.Context.GetValue(NotificationConstants.Parameter.OrderId).Should().Be("ORD-1");
        result.Value.Context.GetValue(NotificationConstants.Parameter.OtpCode).Should().Be("123");
    }

    [Fact(DisplayName = "Context Semantic Helpers should set correct params")]
    public void Context_SemanticHelpers_ShouldWork()
    {
        var result = NotificationMessageBuilder.CreateContext()
            .ContextWithUserFirstName("Bob")
            .ContextWithUserEmail("bob@test.com")
            .ContextWithOrderId("ORD-1")
            .ContextWithSystemName("System")
            .ContextWithOtpCode("123");

        result.Value.GetValue(NotificationConstants.Parameter.UserFirstName).Should().Be("Bob");
        result.Value.GetValue(NotificationConstants.Parameter.UserEmail).Should().Be("bob@test.com");
        result.Value.GetValue(NotificationConstants.Parameter.OrderId).Should().Be("ORD-1");
        result.Value.GetValue(NotificationConstants.Parameter.SystemName).Should().Be("System");
        result.Value.GetValue(NotificationConstants.Parameter.OtpCode).Should().Be("123");
    }
    #endregion

    #region Edge Case Tests
    [Fact(DisplayName = "AddParam should overwrite existing value (list-based last-one-wins)")]
    public void AddParam_Overwrite_ShouldWork()
    {
        var result = NotificationMessageBuilder.CreateContext()
            .AddParam(NotificationConstants.Parameter.OrderId, "OLD")
            .AddParam(NotificationConstants.Parameter.OrderId, "NEW");

        result.Value.GetValue(NotificationConstants.Parameter.OrderId).Should().Be("NEW");
        // Internal check: Ensure we don't have duplicates in the list if ApplyParameter handles it
        result.Value.Parameters.Count(p => p.Key == NotificationConstants.Parameter.OrderId).Should().Be(1);
    }

    [Fact(DisplayName = "Adding null value should be stored as null")]
    public void AddParam_NullValue_ShouldWork()
    {
        var result = NotificationMessageBuilder.CreateContext()
            .AddParam(NotificationConstants.Parameter.OrderId, null);

        result.Value.GetValue(NotificationConstants.Parameter.OrderId).Should().BeNull();
        result.Value.Parameters.Should().ContainSingle(p => p.Key == NotificationConstants.Parameter.OrderId);
    }
    #endregion
}