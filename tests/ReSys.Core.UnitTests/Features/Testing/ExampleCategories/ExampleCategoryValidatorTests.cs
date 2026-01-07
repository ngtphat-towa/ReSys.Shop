using FluentValidation.TestHelper;


using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;

namespace ReSys.Core.UnitTests.Features.Testing.ExampleCategories;

public class ExampleCategoryValidatorTests
{
    private readonly CreateExampleCategory.Validator _validator;

    public ExampleCategoryValidatorTests()
    {
        _validator = new CreateExampleCategory.Validator();
    }

    [Fact(DisplayName = "Validator: Should fail when name is empty")]
    public void Validator_EmptyName_ShouldHaveError()
    {
        var command = new CreateExampleCategory.Command(new CreateExampleCategory.Request { Name = "" });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Name");
    }

    [Fact(DisplayName = "Validator: Should fail when name exceeds max length")]
    public void Validator_NameTooLong_ShouldHaveError()
    {
        var command = new CreateExampleCategory.Command(new CreateExampleCategory.Request 
        { 
            Name = new string('A', ExampleCategoryConstraints.NameMaxLength + 1) 
        });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Name");
    }

    [Fact(DisplayName = "Validator: Should fail when description exceeds max length")]
    public void Validator_DescriptionTooLong_ShouldHaveError()
    {
        var command = new CreateExampleCategory.Command(new CreateExampleCategory.Request 
        { 
            Name = "Valid Name",
            Description = new string('A', ExampleCategoryConstraints.DescriptionMaxLength + 1) 
        });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Request.Description");
    }

    [Fact(DisplayName = "Validator: Should pass for valid request")]
    public void Validator_ValidRequest_ShouldNotHaveErrors()
    {
        var command = new CreateExampleCategory.Command(new CreateExampleCategory.Request 
        { 
            Name = "Valid Category",
            Description = "Valid Description"
        });
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
