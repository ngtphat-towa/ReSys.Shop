using FluentValidation;

namespace ReSys.Core.Features.Admin.Catalog.Products.Properties.Common;

public class ProductPropertyValidator : AbstractValidator<ProductPropertyParameters>
{
    public ProductPropertyValidator()
    {
        RuleFor(x => x.PropertyTypeId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
    }
}
