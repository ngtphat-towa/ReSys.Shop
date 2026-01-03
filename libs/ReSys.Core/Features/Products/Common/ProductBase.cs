using FluentValidation;
using ReSys.Core.Entities;
using System.Linq.Expressions;

namespace ReSys.Core.Features.Products.Common;

// Base properties shared by Input and Detail
public record ProductBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

// Input for Create/Update
public record ProductInput : ProductBase
{
}

// Read model for List (Lightweight)
public record ProductListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public static Expression<Func<Product, ProductListItem>> Projection => x => new ProductListItem
    {
        Id = x.Id,
        Name = x.Name,
        Price = x.Price,
        ImageUrl = x.ImageUrl
    };
}

// Read model for Detail (Full)
public record ProductDetail : ProductBase
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static Expression<Func<Product, ProductDetail>> Projection => x => new ProductDetail
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        Price = x.Price,
        ImageUrl = x.ImageUrl,
        CreatedAt = x.CreatedAt
    };
}

public abstract class ProductValidator<T> : AbstractValidator<T> where T : ProductBase
{
    protected ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(ProductErrors.NameRequired.Code)
                .WithMessage(ProductErrors.NameRequired.Description)
            .MaximumLength(ProductConstraints.NameMaxLength)
                .WithErrorCode(ProductErrors.NameTooLong.Code)
                .WithMessage(ProductErrors.NameTooLong.Description);

        RuleFor(x => x.Description)
            .NotEmpty()
                .WithErrorCode(ProductErrors.DescriptionRequired.Code)
                .WithMessage(ProductErrors.DescriptionRequired.Description);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(ProductConstraints.MinPrice)
                .WithErrorCode(ProductErrors.InvalidPrice.Code)
                .WithMessage(ProductErrors.InvalidPrice.Description);
    }
}
