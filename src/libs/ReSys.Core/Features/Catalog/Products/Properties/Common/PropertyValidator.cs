using FluentValidation;

namespace ReSys.Core.Features.Catalog.Products.Properties.Common;

public class ProductPropertyValidator : AbstractValidator<ProductPropertyParameters>
{
    public ProductPropertyValidator()
    {
        RuleFor(x => x.PropertyTypeId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
    }
}
