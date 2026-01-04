using FluentValidation;
using ReSys.Core.Entities;
using System.Linq.Expressions;

namespace ReSys.Core.Features.Examples.Common;

// Base properties shared by Input and Detail
public record ExampleBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

// Input for Create/Update
public record ExampleInput : ExampleBase
{
}

// Read model for List (Lightweight)
public record ExampleListItem : ExampleBase
{
    public Guid Id { get; set; }

    public static Expression<Func<Example, ExampleListItem>> Projection => x => new ExampleListItem
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        Price = x.Price,
        ImageUrl = x.ImageUrl
    };
}

// Read model for Detail (Full)
public record ExampleDetail : ExampleBase
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static Expression<Func<Example, ExampleDetail>> Projection => x => new ExampleDetail
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        Price = x.Price,
        ImageUrl = x.ImageUrl,
        CreatedAt = x.CreatedAt
    };
}

public abstract class ExampleValidator<T> : AbstractValidator<T> where T : ExampleBase
{
    protected ExampleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(ExampleErrors.NameRequired.Code)
                .WithMessage(ExampleErrors.NameRequired.Description)
            .MaximumLength(ExampleConstraints.NameMaxLength)
                .WithErrorCode(ExampleErrors.NameTooLong.Code)
                .WithMessage(ExampleErrors.NameTooLong.Description);

        RuleFor(x => x.Description)
            .NotEmpty()
                .WithErrorCode(ExampleErrors.DescriptionRequired.Code)
                .WithMessage(ExampleErrors.DescriptionRequired.Description);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(ExampleConstraints.MinPrice)
                .WithErrorCode(ExampleErrors.InvalidPrice.Code)
                .WithMessage(ExampleErrors.InvalidPrice.Description);
    }
}
