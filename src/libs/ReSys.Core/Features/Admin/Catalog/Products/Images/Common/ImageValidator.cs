using FluentValidation;
using ReSys.Core.Domain.Catalog.Products.Images;

namespace ReSys.Core.Features.Admin.Catalog.Products.Images.Common;

public class ProductImageValidator : AbstractValidator<ProductImageParameters>
{
    public ProductImageValidator()
    {
        RuleFor(x => x.Alt)
            .MaximumLength(ProductImageConstraints.AltMaxLength);

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0);
    }
}
