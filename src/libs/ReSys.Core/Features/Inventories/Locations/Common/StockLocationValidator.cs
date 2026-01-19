using FluentValidation;

using ReSys.Core.Domain.Inventories.Locations;

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
            .NotEmpty()
            .WithErrorCode(StockLocationErrors.NameRequired.Code)
            .WithMessage(StockLocationErrors.NameRequired.Description)
            .MaximumLength(StockLocationConstraints.NameMaxLength)
            .WithErrorCode(StockLocationErrors.NameTooLong.Code)
            .WithMessage(StockLocationErrors.NameTooLong.Description);

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode(StockLocationErrors.CodeRequired.Code)
            .WithMessage(StockLocationErrors.CodeRequired.Description)
            .Matches(StockLocationConstraints.CodeRegex)
            .WithErrorCode(StockLocationErrors.InvalidCodeFormat.Code)
            .WithMessage(StockLocationErrors.InvalidCodeFormat.Description)
            .MaximumLength(StockLocationConstraints.CodeMaxLength)
            .WithErrorCode(StockLocationErrors.CodeTooLong.Code)
            .WithMessage(StockLocationErrors.CodeTooLong.Description);

        RuleFor(x => x.Presentation)
            .MaximumLength(StockLocationConstraints.PresentationMaxLength)
            .WithErrorCode(StockLocationErrors.PresentationTooLong.Code)
            .WithMessage(StockLocationErrors.PresentationTooLong.Description);

        RuleFor(x => x.Type)
        .IsInEnum()
            .WithErrorCode(StockLocationErrors.TypeRequired.Code)
            .WithMessage(StockLocationErrors.TypeRequired.Description)
        .Must(type => Enum.IsDefined(typeof(StockLocationType), type))
            .WithErrorCode(StockLocationErrors.InvalidType.Code)
            .WithMessage(StockLocationErrors.InvalidType.Description);
    }
}

public class StockLocationInputValidator : StockLocationParametersValidator<StockLocationInput>
{
    public StockLocationInputValidator()
    {
        RuleFor(x => x.Address).SetValidator(new AddressInputValidator());
    }
}
