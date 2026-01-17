using FluentValidation;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Common.Constraints;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;

public abstract class TaxonValidator<T> : AbstractValidator<T> where T : TaxonParameters
{
    protected TaxonValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(TaxonErrors.NameRequired.Code)
                .WithMessage(TaxonErrors.NameRequired.Description)
            .MaximumLength(CommonConstraints.NameMaxLength)
                .WithErrorCode(TaxonErrors.NameTooLong.Code)
                .WithMessage(TaxonErrors.NameTooLong.Description);

        RuleFor(x => x.Presentation)
            .NotEmpty()
                .WithErrorCode(TaxonErrors.PresentationRequired.Code)
                .WithMessage(TaxonErrors.PresentationRequired.Description)
            .MaximumLength(CommonConstraints.PresentationMaxLength)
                .WithErrorCode(TaxonErrors.PresentationTooLong.Code)
                .WithMessage(TaxonErrors.PresentationTooLong.Description);

        RuleFor(x => x.Slug)
            .NotEmpty()
                .WithErrorCode(TaxonErrors.SlugRequired.Code)
                .WithMessage(TaxonErrors.SlugRequired.Description)
            .MaximumLength(CommonConstraints.SlugMaxLength)
                .WithErrorCode(TaxonErrors.SlugTooLong.Code)
                .WithMessage(TaxonErrors.SlugTooLong.Description);

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0);
    }
}

public class TaxonInputValidator : TaxonValidator<TaxonInput>
{
    public TaxonInputValidator()
    {
        RuleFor(x => x.RulesMatchPolicy)
            .Must(x => x == "all" || x == "any")
            .When(x => x.Automatic)
            .WithErrorCode(TaxonErrors.InvalidRulesMatchPolicy.Code)
            .WithMessage(TaxonErrors.InvalidRulesMatchPolicy.Description);

        RuleFor(x => x.SortOrder)
            .NotEmpty()
            .When(x => x.Automatic)
            .WithErrorCode(TaxonErrors.SortOrderRequired.Code)
            .WithMessage(TaxonErrors.SortOrderRequired.Description);

        RuleFor(x => x.MetaTitle)
            .MaximumLength(ProductConstraints.Seo.MetaTitleMaxLength);

        RuleFor(x => x.MetaDescription)
            .MaximumLength(ProductConstraints.Seo.MetaDescriptionMaxLength);

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(ProductConstraints.Seo.MetaKeywordsMaxLength);

        // Metadata validation
        this.AddMetadataValidationRules(x => x.PublicMetadata);
        this.AddMetadataValidationRules(x => x.PrivateMetadata);
    }
}