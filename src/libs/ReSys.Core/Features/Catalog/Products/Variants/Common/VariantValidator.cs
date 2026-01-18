using FluentValidation;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Features.Catalog.Products.Variants.Common;

public abstract class VariantParametersValidator<T> : AbstractValidator<T> where T : VariantParameters
{
    protected VariantParametersValidator()
    {
        RuleFor(x => x.Sku)
            .MaximumLength(VariantConstraints.SkuMaxLength)
            .WithErrorCode(VariantErrors.SkuTooLong.Code)
            .WithMessage(VariantErrors.SkuTooLong.Description);

        RuleFor(x => x.Barcode)
            .MaximumLength(VariantConstraints.BarcodeMaxLength)
            .WithErrorCode(VariantErrors.BarcodeTooLong.Code)
            .WithMessage(VariantErrors.BarcodeTooLong.Description);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(VariantConstraints.MinPrice)
            .WithErrorCode(VariantErrors.InvalidPrice.Code)
            .WithMessage(VariantErrors.InvalidPrice.Description);

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(x => x.Price)
            .When(x => x.CompareAtPrice.HasValue)
            .WithErrorCode(VariantErrors.InvalidCompareAtPrice.Code)
            .WithMessage(VariantErrors.InvalidCompareAtPrice.Description);

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(VariantConstraints.Dimensions.MinValue)
            .When(x => x.Weight.HasValue)
            .WithErrorCode(VariantErrors.InvalidDimension.Code)
            .WithMessage(VariantErrors.InvalidDimension.Description);

        RuleFor(x => x.Height)
            .GreaterThanOrEqualTo(VariantConstraints.Dimensions.MinValue)
            .When(x => x.Height.HasValue)
            .WithErrorCode(VariantErrors.InvalidDimension.Code)
            .WithMessage(VariantErrors.InvalidDimension.Description);

        RuleFor(x => x.Width)
            .GreaterThanOrEqualTo(VariantConstraints.Dimensions.MinValue)
            .When(x => x.Width.HasValue)
            .WithErrorCode(VariantErrors.InvalidDimension.Code)
            .WithMessage(VariantErrors.InvalidDimension.Description);

        RuleFor(x => x.Depth)
            .GreaterThanOrEqualTo(VariantConstraints.Dimensions.MinValue)
            .When(x => x.Depth.HasValue)
            .WithErrorCode(VariantErrors.InvalidDimension.Code)
            .WithMessage(VariantErrors.InvalidDimension.Description);

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(VariantConstraints.MinPosition);
    }
}

public class VariantInputValidator : VariantParametersValidator<VariantInput>
{
    public VariantInputValidator()
    {
        this.AddMetadataValidationRules(x => x.PublicMetadata);
        this.AddMetadataValidationRules(x => x.PrivateMetadata);
    }
}