using FluentValidation.TestHelper;

using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.OptionValues.Common;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionValues")]
public class OptionValueValidatorTests
{
    private readonly TestValidator _validator;

    public OptionValueValidatorTests()
    {
        _validator = new TestValidator();
    }

    private class TestValidator : OptionValueValidator<OptionValueInput> { }

    [Fact(DisplayName = "Validator: Should fail when name is empty")]
    public void Validator_EmptyName_ShouldHaveError()
    {
        var model = new OptionValueInput { Name = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(OptionValueErrors.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when presentation is empty")]
    public void Validator_EmptyPresentation_ShouldHaveError()
    {
        var model = new OptionValueInput { Name = "Valid", Presentation = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(OptionValueErrors.PresentationRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when position is negative")]
    public void Validator_NegativePosition_ShouldHaveError()
    {
        var model = new OptionValueInput
        {
            Name = "Valid",
            Presentation = "Valid",
            Position = -1
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(OptionValueErrors.InvalidPosition.Code);
    }

    [Fact(DisplayName = "Validator: Should pass for valid input")]
    public void Validator_ValidInput_ShouldNotHaveErrors()
    {
        var model = new OptionValueInput { Name = "Red", Presentation = "Bright Red" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
