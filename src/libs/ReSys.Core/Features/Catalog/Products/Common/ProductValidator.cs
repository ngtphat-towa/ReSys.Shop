using FluentValidation;

using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Common.Constraints;

namespace ReSys.Core.Features.Catalog.Products.Common;

public abstract class ProductParametersValidator<T> : AbstractValidator<T> where T : ProductParameters
{
    protected ProductParametersValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(ProductErrors.NameRequired.Code)
                .WithMessage(ProductErrors.NameRequired.Description)
            .MaximumLength(ProductConstraints.NameMaxLength)
                .WithErrorCode(ProductErrors.NameTooLong.Code)
                .WithMessage(ProductErrors.NameTooLong.Description);

        RuleFor(x => x.Presentation)
            .NotEmpty()
                .WithErrorCode(ProductErrors.NameRequired.Code)
                .WithMessage(ProductErrors.NameRequired.Description)
            .MaximumLength(ProductConstraints.PresentationMaxLength)
                .WithErrorCode(ProductErrors.PresentationTooLong.Code)
                .WithMessage(ProductErrors.PresentationTooLong.Description);

        RuleFor(x => x.Description)
            .MaximumLength(ProductConstraints.DescriptionMaxLength)
                .WithErrorCode(ProductErrors.DescriptionTooLong.Code)
                .WithMessage(ProductErrors.DescriptionTooLong.Description)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Slug)
            .MaximumLength(ProductConstraints.SlugMaxLength)
                .WithErrorCode(ProductErrors.InvalidSlug.Code)
                .WithMessage(ProductErrors.InvalidSlug.Description)
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.DiscontinuedOn)
            .Must((obj, discontinueOn) =>
                !discontinueOn.HasValue ||
                !obj.MakeActiveAt.HasValue ||
                discontinueOn.Value >= obj.MakeActiveAt.Value)
                .WithErrorCode(ProductErrors.InvalidDiscontinuedOn.Code)
                .WithMessage(ProductErrors.InvalidDiscontinuedOn.Description);
    }
}

public class ProductInputValidator : ProductParametersValidator<ProductInput>
{
    public ProductInputValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(VariantConstraints.SkuMaxLength);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(x => x.MetaTitle)
            .MaximumLength(ProductConstraints.Seo.MetaTitleMaxLength)
                .WithErrorCode(ProductErrors.Seo.MetaTitleTooLong.Code)
                .WithMessage(ProductErrors.Seo.MetaTitleTooLong.Description);

        RuleFor(x => x.MetaDescription)
            .MaximumLength(ProductConstraints.Seo.MetaDescriptionMaxLength)
                .WithErrorCode(ProductErrors.Seo.MetaDescriptionTooLong.Code)
                .WithMessage(ProductErrors.Seo.MetaDescriptionTooLong.Description);
        
        RuleFor(x => x.MetaKeywords)
            .MaximumLength(ProductConstraints.Seo.MetaKeywordsMaxLength)
                .WithErrorCode(ProductErrors.Seo.MetaKeywordsTooLong.Code)
                .WithMessage(ProductErrors.Seo.MetaKeywordsTooLong.Description);

        // Metadata validation
        this.AddMetadataValidationRules(x => x.PublicMetadata);
        this.AddMetadataValidationRules(x => x.PrivateMetadata);
    }
}