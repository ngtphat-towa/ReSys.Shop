using FluentValidation;
using ReSys.Core.Domain.Settings.ShippingMethods;

namespace ReSys.Core.Features.Admin.Settings.ShippingMethods.Common;

public abstract class ShippingMethodParametersValidator<T> : AbstractValidator<T> where T : ShippingMethodParameters
{
    protected ShippingMethodParametersValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ShippingMethodErrors.NameRequired.Code)
            .WithMessage(ShippingMethodErrors.NameRequired.Description)
            .MaximumLength(ShippingMethodConstraints.NameMaxLength)
            .WithErrorCode(ShippingMethodErrors.NameTooLong.Code)
            .WithMessage(ShippingMethodErrors.NameTooLong.Description);

        RuleFor(x => x.Presentation)
            .MaximumLength(ShippingMethodConstraints.PresentationMaxLength)
            .WithErrorCode(ShippingMethodErrors.PresentationTooLong.Code)
            .WithMessage(ShippingMethodErrors.PresentationTooLong.Description);

        RuleFor(x => x.Description)
            .MaximumLength(ShippingMethodConstraints.DescriptionMaxLength)
            .WithErrorCode(ShippingMethodErrors.DescriptionTooLong.Code)
            .WithMessage(ShippingMethodErrors.DescriptionTooLong.Description);

        RuleFor(x => x.BaseCost)
            .GreaterThanOrEqualTo(ShippingMethodConstraints.MinCost)
            .WithErrorCode(ShippingMethodErrors.CostNegative.Code)
            .WithMessage(ShippingMethodErrors.CostNegative.Description);

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}

public class ShippingMethodInputValidator : ShippingMethodParametersValidator<ShippingMethodInput>
{
}
