using FluentValidation;

using ReSys.Core.Domain.Testing.Examples;

namespace ReSys.Core.Features.Testing.Examples.Common;

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
            .MaximumLength(ExampleConstraints.DescriptionMaxLength)
                .WithErrorCode(ExampleErrors.DescriptionTooLong.Code)
                .WithMessage(ExampleErrors.DescriptionTooLong.Description);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(ExampleConstraints.MinPrice)
                .WithErrorCode(ExampleErrors.InvalidPrice.Code)
                .WithMessage(ExampleErrors.InvalidPrice.Description);

        RuleFor(x => x.HexColor)
            .Matches(ExampleConstraints.HexColorRegex)
                .WithErrorCode(ExampleErrors.InvalidHexColor.Code)
                .WithMessage(ExampleErrors.InvalidHexColor.Description)
            .MaximumLength(ExampleConstraints.HexColorMaxLength)
                .WithErrorCode(ExampleErrors.InvalidHexColor.Code)
                .WithMessage(ExampleErrors.InvalidHexColor.Description)
            .When(x => !string.IsNullOrEmpty(x.HexColor));
    }
}
