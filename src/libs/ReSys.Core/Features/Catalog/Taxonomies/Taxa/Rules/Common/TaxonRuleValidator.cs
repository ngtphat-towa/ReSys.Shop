using FluentValidation;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;

public abstract class TaxonRuleValidator<T> : AbstractValidator<T> where T : TaxonRuleParameters
{
    protected TaxonRuleValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(x => TaxonRuleConstraints.RuleTypes.Contains(x.ToLowerInvariant()))
            .WithErrorCode(TaxonRuleErrors.InvalidType.Code)
            .WithMessage(TaxonRuleErrors.InvalidType.Description);

        RuleFor(x => x.MatchPolicy)
            .NotEmpty()
            .Must(x => TaxonRuleConstraints.MatchPolicies.Contains(x.ToLowerInvariant()))
            .WithErrorCode(TaxonRuleErrors.InvalidMatchPolicy.Code)
            .WithMessage(TaxonRuleErrors.InvalidMatchPolicy.Description);

        RuleFor(x => x.PropertyName)
            .NotEmpty()
            .When(x => x.Type?.ToLowerInvariant() == "product_property")
            .WithErrorCode(TaxonRuleErrors.PropertyNameRequired.Code)
            .WithMessage(TaxonRuleErrors.PropertyNameRequired.Description);

        RuleFor(x => x.Value)
            .NotEmpty()
                .WithErrorCode(TaxonRuleErrors.ValueRequired.Code)
                .WithMessage(TaxonRuleErrors.ValueRequired.Description)
            .MaximumLength(TaxonRuleConstraints.ValueMaxLength)
                .WithErrorCode(TaxonRuleErrors.ValueTooLong.Code)
                .WithMessage(TaxonRuleErrors.ValueTooLong.Description);
    }
}

public class TaxonRuleInputValidator : TaxonRuleValidator<TaxonRuleInput> { }