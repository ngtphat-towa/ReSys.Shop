using FluentValidation;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.Features.Inventories.Locations.Common;

public class AddressInputValidator : AbstractValidator<AddressInput>
{
    public AddressInputValidator()
    {
        RuleFor(x => x.Address1).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.ZipCode).NotEmpty();
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
    }
}

public abstract class StockLocationParametersValidator<T> : AbstractValidator<T> where T : StockLocationParameters
{
    protected StockLocationParametersValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(StockLocationErrors.NameRequired.Code)
            .MaximumLength(StockLocationConstraints.NameMaxLength).WithErrorCode(StockLocationErrors.NameTooLong.Code);

        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode(StockLocationErrors.CodeRequired.Code)
            .Matches(StockLocationConstraints.CodeRegex).WithErrorCode(StockLocationErrors.InvalidCodeFormat.Code);
            
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class StockLocationInputValidator : StockLocationParametersValidator<StockLocationInput>
{
    public StockLocationInputValidator()
    {
        RuleFor(x => x.Address).SetValidator(new AddressInputValidator());
    }
}
