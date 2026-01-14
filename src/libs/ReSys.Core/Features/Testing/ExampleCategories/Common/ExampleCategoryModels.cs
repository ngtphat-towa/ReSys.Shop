using System.Linq.Expressions;

using ReSys.Core.Domain.Testing.ExampleCategories;

namespace ReSys.Core.Features.Testing.ExampleCategories.Common;

public record ExampleCategoryBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public record ExampleCategoryListItem : ExampleCategoryBase
{
    public Guid Id { get; set; }

    public static Expression<Func<ExampleCategory, ExampleCategoryListItem>> Projection => x => new ExampleCategoryListItem
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description
    };
}

public record ExampleCategoryDetail : ExampleCategoryBase
{
    public Guid Id { get; set; }

    public static Expression<Func<ExampleCategory, ExampleCategoryDetail>> Projection => x => new ExampleCategoryDetail
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description
    };
}
