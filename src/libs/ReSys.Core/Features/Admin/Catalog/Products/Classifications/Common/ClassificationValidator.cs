using FluentValidation;

namespace ReSys.Core.Features.Admin.Catalog.Products.Classifications.Common;

public class ProductClassificationValidator : AbstractValidator<ProductClassificationParameters>
{
    public ProductClassificationValidator()
    {
        RuleFor(x => x.TaxonId).NotEmpty();
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
