using FluentValidation;

using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Common.Constraints;

namespace ReSys.Core.Features.Catalog.Taxonomies.Common;

public abstract class TaxonomyValidator<T> : AbstractValidator<T> where T : TaxonomyParameters
{
    protected TaxonomyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(TaxonomyErrors.NameRequired.Code)
                .WithMessage(TaxonomyErrors.NameRequired.Description)
            .MaximumLength(CommonConstraints.NameMaxLength)
                .WithErrorCode(TaxonomyErrors.NameTooLong.Code)
                .WithMessage(TaxonomyErrors.NameTooLong.Description);

        RuleFor(x => x.Presentation)
            .NotEmpty()
                .WithErrorCode(TaxonomyErrors.PresentationRequired.Code)
                .WithMessage(TaxonomyErrors.PresentationRequired.Description)
            .MaximumLength(CommonConstraints.PresentationMaxLength)
                .WithErrorCode(TaxonomyErrors.PresentationTooLong.Code)
                .WithMessage(TaxonomyErrors.PresentationTooLong.Description);

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(TaxonomyConstraints.MinPosition)
                .WithErrorCode(TaxonomyErrors.InvalidPosition.Code)
                .WithMessage(TaxonomyErrors.InvalidPosition.Description);
    }
}

public class TaxonomyInputValidator : TaxonomyValidator<TaxonomyInput>
{
    public TaxonomyInputValidator()
    {
        // Metadata validation
        this.AddMetadataValidationRules(x => x.PublicMetadata);
        this.AddMetadataValidationRules(x => x.PrivateMetadata);
    }
}