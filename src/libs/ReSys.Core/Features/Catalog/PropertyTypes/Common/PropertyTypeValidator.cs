using FluentValidation;
using ReSys.Core.Domain.Catalog.PropertyTypes;

namespace ReSys.Core.Features.Catalog.PropertyTypes.Common;

public abstract class PropertyTypeValidator<T> : AbstractValidator<T> where T : PropertyTypeParameters
{
    protected PropertyTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(PropertyTypeErrors.NameRequired.Code)
                .WithMessage(PropertyTypeErrors.NameRequired.Description)
            .MaximumLength(PropertyTypeConstraints.NameMaxLength)
                .WithErrorCode(PropertyTypeErrors.NameTooLong.Code)
                .WithMessage(PropertyTypeErrors.NameTooLong.Description);

        RuleFor(x => x.Presentation)
            .NotEmpty()
                .WithErrorCode(PropertyTypeErrors.PresentationRequired.Code)
                .WithMessage(PropertyTypeErrors.PresentationRequired.Description)
            .MaximumLength(PropertyTypeConstraints.PresentationMaxLength)
                .WithErrorCode(PropertyTypeErrors.PresentationTooLong.Code)
                .WithMessage(PropertyTypeErrors.PresentationTooLong.Description);

        RuleFor(x => x.Kind)
            .IsInEnum()
                .WithErrorCode(PropertyTypeErrors.InvalidKind.Code)
                .WithMessage(PropertyTypeErrors.InvalidKind.Description);

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(PropertyTypeConstraints.MinPosition)
                .WithErrorCode(PropertyTypeErrors.InvalidPosition.Code)
                .WithMessage(PropertyTypeErrors.InvalidPosition.Description);
    }
}
