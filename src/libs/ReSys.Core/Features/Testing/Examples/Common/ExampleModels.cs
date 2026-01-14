using ReSys.Core.Domain.Testing.Examples;

using System.Linq.Expressions;

namespace ReSys.Core.Features.Testing.Examples.Common;

// Base properties shared by Input and Detail
public record ExampleBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public ExampleStatus Status { get; set; }
    public string? HexColor { get; set; }
    public Guid? CategoryId { get; set; }
}

// Input for Create/Update
public record ExampleInput : ExampleBase
{
}

// Read model for List (Lightweight)
public record ExampleListItem : ExampleBase
{
    public Guid Id { get; set; }
    public string? CategoryName { get; set; }

    public static Expression<Func<Example, ExampleListItem>> Projection => x => new ExampleListItem
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        Price = x.Price,
        ImageUrl = x.ImageUrl,
        Status = x.Status,
        HexColor = x.HexColor,
        CategoryId = x.CategoryId,
        CategoryName = x.Category != null ? x.Category.Name : null
    };
}

// Read model for Detail (Full)
public record ExampleDetail : ExampleBase
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CategoryName { get; set; }

    public static Expression<Func<Example, ExampleDetail>> Projection => x => new ExampleDetail
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        Price = x.Price,
        ImageUrl = x.ImageUrl,
        Status = x.Status,
        HexColor = x.HexColor,
        CreatedAt = x.CreatedAt,
        CategoryId = x.CategoryId,
        CategoryName = x.Category != null ? x.Category.Name : null
    };
}
