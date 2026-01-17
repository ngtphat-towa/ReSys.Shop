using FluentValidation;


using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;

public abstract class OptionValueValidator<T> : AbstractValidator<T> where T : OptionValueParameters
{
    protected OptionValueValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(OptionValueErrors.NameRequired.Code)
                .WithMessage(OptionValueErrors.NameRequired.Description)
            .MaximumLength(OptionValueConstraints.NameMaxLength)
                .WithErrorCode(OptionValueErrors.NameTooLong.Code)
                .WithMessage(OptionValueErrors.NameTooLong.Description);

        RuleFor(x => x.Presentation)
            .NotEmpty()
                .WithErrorCode(OptionValueErrors.PresentationRequired.Code)
                .WithMessage(OptionValueErrors.PresentationRequired.Description)
            .MaximumLength(OptionValueConstraints.PresentationMaxLength)
                .WithErrorCode(OptionValueErrors.PresentationTooLong.Code)
                .WithMessage(OptionValueErrors.PresentationTooLong.Description);
    }
}

public class OptionValueInputValidator : OptionValueValidator<OptionValueInput> { }