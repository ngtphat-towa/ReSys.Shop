using FluentValidation.TestHelper;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Features.Catalog.OptionTypes.Common;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.Common;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class OptionTypeValidatorTests
{
    private readonly TestOptionTypeValidator _validator;

    public OptionTypeValidatorTests()
    {
        _validator = new TestOptionTypeValidator();
    }

    public class TestOptionTypeValidator : OptionTypeInputValidator
    {
        public TestOptionTypeValidator() : base() { }
    }

    [Fact(DisplayName = "Validator: Should fail when name is empty")]
    public void Validator_EmptyName_ShouldHaveError()
    {
        var model = new OptionTypeInput { Name = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(OptionTypeErrors.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when name exceeds max length")]
    public void Validator_NameTooLong_ShouldHaveError()
    {
        var model = new OptionTypeInput { Name = new string('A', OptionTypeConstraints.NameMaxLength + 1) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(OptionTypeErrors.NameTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when presentation exceeds max length")]
    public void Validator_PresentationTooLong_ShouldHaveError()
    {
        var model = new OptionTypeInput
        {
            Name = "Valid Name",
            Presentation = new string('A', OptionTypeConstraints.PresentationMaxLength + 1)
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(OptionTypeErrors.PresentationTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when public metadata has too many items")]
    public void Validator_TooManyPublicMetadataItems_ShouldHaveError()
    {
        var metadata = new Dictionary<string, object?>();
        for (int i = 0; i <= HasMetadataConstraints.MaxMetadataItems; i++)
        {
            metadata.Add($"key{i}", "value");
        }

        var model = new OptionTypeInput
        {
            Name = "Valid Name",
            PublicMetadata = metadata
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PublicMetadata)
            .WithErrorMessage(MetadataErrors.TooManyItems.Description);
    }

    [Fact(DisplayName = "Validator: Should fail when private metadata has invalid key")]
    public void Validator_InvalidPrivateMetadataKey_ShouldHaveError()
    {
        var model = new OptionTypeInput
        {
            Name = "Valid Name",
            PrivateMetadata = new Dictionary<string, object?> { [""] = "value" }
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PrivateMetadata)
            .WithErrorMessage(MetadataErrors.KeyRequired.Description);
    }

    [Fact(DisplayName = "Validator: Should fail when metadata value type is invalid")]
    public void Validator_InvalidMetadataValueType_ShouldHaveError()
    {
        var model = new OptionTypeInput
        {
            Name = "Valid Name",
            PublicMetadata = new Dictionary<string, object?> { ["key"] = new { Complex = "Object" } }
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PublicMetadata)
            .WithErrorMessage(MetadataErrors.InvalidType.Description);
    }

    [Fact(DisplayName = "Validator: Should pass for valid input")]
    public void Validator_ValidInput_ShouldNotHaveErrors()
    {
        var model = new OptionTypeInput
        {
            Name = "Valid Name",
            Presentation = "Valid Presentation",
            PublicMetadata = new Dictionary<string, object?> { ["key"] = "value", ["count"] = 10 },
            PrivateMetadata = new Dictionary<string, object?> { ["secret"] = true }
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
