using FluentValidation;


namespace ReSys.Core.Features.Testing.ExampleCategories.Common;

public abstract class ExampleCategoryValidator<T> : AbstractValidator<T> where T : ExampleCategoryBase
{
    protected ExampleCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(ExampleCategoryErrors.NameRequired.Code)
                .WithMessage(ExampleCategoryErrors.NameRequired.Description)
            .MaximumLength(ExampleCategoryConstraints.NameMaxLength)
                .WithErrorCode(ExampleCategoryErrors.NameTooLong.Code)
                .WithMessage(ExampleCategoryErrors.NameTooLong.Description);

        RuleFor(x => x.Description)
            .MaximumLength(ExampleCategoryConstraints.DescriptionMaxLength)
                .WithErrorCode(ExampleCategoryErrors.DescriptionTooLong.Code)
                .WithMessage(ExampleCategoryErrors.DescriptionTooLong.Description);
    }
}
