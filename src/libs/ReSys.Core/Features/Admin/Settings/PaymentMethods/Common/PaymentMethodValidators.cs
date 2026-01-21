using FluentValidation;
using ReSys.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods.Common;

public abstract class PaymentMethodParametersValidator<T> : AbstractValidator<T> where T : PaymentMethodParameters
{
    protected PaymentMethodParametersValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(PaymentMethodErrors.NameRequired.Code)
            .WithMessage(PaymentMethodErrors.NameRequired.Description)
            .MaximumLength(PaymentMethodConstraints.NameMaxLength)
            .WithErrorCode(PaymentMethodErrors.NameTooLong.Code)
            .WithMessage(PaymentMethodErrors.NameTooLong.Description);

        RuleFor(x => x.Presentation)
            .MaximumLength(PaymentMethodConstraints.PresentationMaxLength)
            .WithErrorCode(PaymentMethodErrors.PresentationTooLong.Code)
            .WithMessage(PaymentMethodErrors.PresentationTooLong.Description);

        RuleFor(x => x.Description)
            .MaximumLength(PaymentMethodConstraints.DescriptionMaxLength)
            .WithErrorCode(PaymentMethodErrors.DescriptionTooLong.Code)
            .WithMessage(PaymentMethodErrors.DescriptionTooLong.Description);

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}

public class PaymentMethodInputValidator : PaymentMethodParametersValidator<PaymentMethodInput>
{
}
