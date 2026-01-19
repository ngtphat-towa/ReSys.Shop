using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Core.Common.Notifications.Validators;
using ReSys.Core.Common.Notifications.Errors;
using ReSys.Core.Common.Notifications.Builders;

using FluentValidation;

namespace ReSys.Core.UnitTests.Common.Notifications.Validators;

public class NotificationValidatorTests
{
    private readonly NotificationMessageValidator _validator = new();

    [Fact(DisplayName = "Validator should fail when UseCase is None")]
    public async Task Validate_WhenUseCaseNone_ShouldHaveError()
    {
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.None,
            NotificationRecipient.Create("test@example.com")).Value;

        var result = await _validator.ValidateAsync(message, TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == NotificationErrors.General.UseCaseRequired.Code);
    }

    [Fact(DisplayName = "Validator should fail when Recipient is null")]
    public async Task Validate_WhenRecipientNull_ShouldHaveError()
    {
        var message = NotificationMessageBuilder.ForUseCase(NotificationConstants.UseCase.SystemOrderConfirmation)
            .To(null!).Value;

        var result = await _validator.ValidateAsync(message, TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == NotificationErrors.General.RecipientRequired.Code);
    }

    [Theory(DisplayName = "Email Validator should handle various formats")]
    [InlineData("valid@email.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    [InlineData("test@domain@domain.com", false)]
    public async Task Validate_EmailFormats(string email, bool expectedValid)
    {
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            NotificationRecipient.Create(email)).Value;

        var result = await _validator.ValidateAsync(message, o => o.IncludeRuleSets("Full"), TestContext.Current.CancellationToken);

        result.IsValid.Should().Be(expectedValid);
        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.ErrorCode == NotificationErrors.Email.InvalidFormat.Code || e.ErrorCode == NotificationErrors.Email.ToRequired.Code);
        }
    }

    [Theory(DisplayName = "SMS Validator should handle various phone formats")]
    [InlineData("+420777123456", true)]
    [InlineData("123456789", true)] 
    [InlineData("+12345678901234567", false)] 
    [InlineData("abc", false)]
    public async Task Validate_PhoneFormats(string phone, bool expectedValid)
    {
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemActivePhone,
            NotificationRecipient.Create(phone)).Value;

        var result = await _validator.ValidateAsync(message, o => o.IncludeRuleSets("Full"), TestContext.Current.CancellationToken);

        result.IsValid.Should().Be(expectedValid);
    }

    [Fact(DisplayName = "Validator should fail when attachment filename is missing")]
    public async Task Validate_WhenAttachmentFilenameMissing_ShouldHaveError()
    {
        var message = NotificationMessageBuilder.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            NotificationRecipient.Create("test@example.com"))
            .AddAttachment(NotificationAttachment.Create("", [0x01], "text/plain"))
            .Value;

        var result = await _validator.ValidateAsync(message, o => o.IncludeRuleSets("Full"), TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == NotificationErrors.Email.AttachmentFileNameRequired.Code);
    }
}
