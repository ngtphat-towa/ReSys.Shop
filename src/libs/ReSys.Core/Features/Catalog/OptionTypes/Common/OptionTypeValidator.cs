using FluentValidation;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Features.Catalog.OptionTypes.Common;

public abstract class OptionTypeValidator<T> : AbstractValidator<T> where T
    : OptionTypeParameters
{
    protected OptionTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(OptionTypeErrors.NameRequired.Code)
                .WithMessage(OptionTypeErrors.NameRequired.Description)
            .MaximumLength(OptionTypeConstraints.NameMaxLength)
                .WithErrorCode(OptionTypeErrors.NameTooLong.Code)
                .WithMessage(OptionTypeErrors.NameTooLong.Description);

        RuleFor(x => x.Presentation)

            .MaximumLength(OptionTypeConstraints.PresentationMaxLength)
                .WithErrorCode(OptionTypeErrors.PresentationTooLong.Code)
                .WithMessage(OptionTypeErrors.PresentationTooLong.Description);
    }
}

public class OptionTypeInputValidator : OptionTypeValidator<OptionTypeInput>
{
    public OptionTypeInputValidator()
    {
        // Metadata validation
        this.AddMetadataValidationRules(x => x.PublicMetadata);
        this.AddMetadataValidationRules(x => x.PrivateMetadata);
    }
}