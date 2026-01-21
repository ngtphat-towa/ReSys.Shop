using FluentValidation;
using ReSys.Core.Domain.Settings.Stores;

namespace ReSys.Core.Features.Admin.Settings.Stores.Common;

public abstract class StoreParametersValidator<T> : AbstractValidator<T> where T : StoreParameters
{
    protected StoreParametersValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(StoreErrors.NameRequired.Code)
            .MaximumLength(StoreConstraints.NameMaxLength);

        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode(StoreErrors.CodeRequired.Code)
            .Matches(StoreConstraints.CodeRegex).WithErrorCode(StoreErrors.InvalidCodeFormat.Code)
            .MaximumLength(StoreConstraints.CodeMaxLength);

        RuleFor(x => x.DefaultCurrency)
            .NotEmpty().WithErrorCode(StoreErrors.CurrencyRequired.Code)
            .Length(StoreConstraints.CurrencyCodeMaxLength);
    }
}

public class StoreInputValidator : StoreParametersValidator<StoreInput> { }
