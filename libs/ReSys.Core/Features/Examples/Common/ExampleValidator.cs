using FluentValidation;

namespace ReSys.Core.Features.Examples.Common;

public abstract class ExampleValidator<T> : AbstractValidator<T> where T : ExampleBase
{
    protected ExampleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(ExampleErrors.NameRequired.Code)
                .WithMessage(ExampleErrors.NameRequired.Description)
            .MaximumLength(ExampleConstraints.NameMaxLength)
                .WithErrorCode(ExampleErrors.NameTooLong.Code)
                .WithMessage(ExampleErrors.NameTooLong.Description);

        RuleFor(x => x.Description)
            .NotEmpty()
                .WithErrorCode(ExampleErrors.DescriptionRequired.Code)
                .WithMessage(ExampleErrors.DescriptionRequired.Description);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(ExampleConstraints.MinPrice)
                .WithErrorCode(ExampleErrors.InvalidPrice.Code)
                .WithMessage(ExampleErrors.InvalidPrice.Description);
    }
}
