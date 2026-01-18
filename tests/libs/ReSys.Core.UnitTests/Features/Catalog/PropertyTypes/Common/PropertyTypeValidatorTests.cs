using FluentValidation.TestHelper;

using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;

namespace ReSys.Core.UnitTests.Features.Catalog.PropertyTypes.Common;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class PropertyTypeValidatorTests
{
    private readonly TestValidator _validator;

    public PropertyTypeValidatorTests()
    {
        _validator = new TestValidator();
    }

    private class TestValidator : PropertyTypeValidator<PropertyTypeInput> { }

    [Fact(DisplayName = "Validator: Should fail when name is empty")]
    public void Validator_EmptyName_ShouldHaveError()
    {
        var model = new PropertyTypeInput { Name = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(PropertyTypeErrors.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when kind is invalid")]
    public void Validator_InvalidKind_ShouldHaveError()
    {
        var model = new PropertyTypeInput { Name = "Valid", Kind = (PropertyKind)999 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Kind)
            .WithErrorCode(PropertyTypeErrors.InvalidKind.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when position is negative")]
    public void Validator_NegativePosition_ShouldHaveError()
    {
        var model = new PropertyTypeInput { Name = "Valid", Position = -1 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(PropertyTypeErrors.InvalidPosition.Code);
    }

    [Fact(DisplayName = "Validator: Should pass for valid input")]
    public void Validator_ValidInput_ShouldNotHaveErrors()
    {
        var model = new PropertyTypeInput 
        { 
            Name = "Material", 
            Presentation = "Material", 
            Kind = PropertyKind.String 
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}