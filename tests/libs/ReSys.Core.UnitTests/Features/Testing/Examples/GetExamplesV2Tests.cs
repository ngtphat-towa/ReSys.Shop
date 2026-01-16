using Microsoft.EntityFrameworkCore;

using ReSys.Shared.Models;
using ReSys.Core.Features.Testing.Examples.GetExamples;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Domain.Testing.ExampleCategories;
using ReSys.Core.Common.Extensions.Query;

namespace ReSys.Core.UnitTests.Features.Testing.Examples;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Examples")]
public class GetExamplesV2Tests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "Handle: Should filter examples by dynamic string correctly")]
    public async Task Handle_FilterProvided_ReturnsCorrectItems()
    {
        // Arrange
        var baseName = $"V2Filter_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Apple", Price = 10 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Banana", Price = 20 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Orange", Price = 30 }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Filter for price > 15 AND name contains "Banana"
        var request = new GetExamplesV2.Request
        {
            Filter = $"Name*{baseName},Price>15,Name*Banana"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Contain("Banana");
    }

    [Fact(DisplayName = "Handle: Should sort examples by dynamic field correctly")]
    public async Task Handle_SortProvided_ReturnsSortedItems()
    {
        // Arrange
        var baseName = $"V2Sort_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_A", Price = 100 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_B", Price = 50 }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Sort by Price ASC
        var request = new GetExamplesV2.Request
        {
            Filter = $"Name*{baseName}",
            Sort = "Price"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items[0].Price.Should().Be(50);
        result.Items[1].Price.Should().Be(100);
    }

    [Fact(DisplayName = "Handle: Should match examples by global search across multiple fields")]
    public async Task Handle_SearchProvided_MatchesFields()
    {
        // Arrange
        var baseName = $"V2Search_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_FindMe", Description = "Test" },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_HideMe", Description = "Other" }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Search for "FindMe"
        var request = new GetExamplesV2.Request
        {
            Search = "FindMe",
            SearchField = new[] { "Name", "Description" }
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Contain("FindMe");
    }

    [Fact(DisplayName = "Handle: Should filter examples by Enum status correctly")]
    public async Task Handle_EnumFilterProvided_ReturnsCorrectItems()
    {
        // Arrange
        var baseName = $"V2Enum_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Active", Status = ExampleStatus.Active },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Draft", Status = ExampleStatus.Draft }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Filter Status=Active
        var request = new GetExamplesV2.Request
        {
            Filter = $"Name*{baseName},Status=Active"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(ExampleStatus.Active);
    }

    [Fact(DisplayName = "Handle: Should filter examples by a list of IDs correctly")]
    public async Task Handle_IdArrayProvided_ReturnsCorrectItems()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        fixture.Context.Set<Example>().AddRange(
            new Example { Id = id1, Name = "Inc1" },
            new Example { Id = id2, Name = "Inc2" },
            new Example { Id = id3, Name = "Exc3" }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Filter (Id=id1|Id=id2)
        var request = new GetExamplesV2.Request
        {
            Filter = $"(Id={id1}|Id={id2})"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Id).Should().Contain([id1, id2]);
        result.Items.Select(x => x.Id).Should().NotContain(id3);
    }

    [Fact(DisplayName = "Handle: Should handle OR logic for multiple statuses correctly")]
    public async Task Handle_StatusOrLogicUsed_WorksCorrectly()
    {
        // Arrange
        var baseName = $"V2StatusOr_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Draft", Status = ExampleStatus.Draft }, // 0
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Active", Status = ExampleStatus.Active }, // 1
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Archived", Status = ExampleStatus.Archived } // 2
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Filter by Name (scope) AND (status=0|status=1)
        var request = new GetExamplesV2.Request
        {
            Filter = $"Name*{baseName},(status={(int)ExampleStatus.Draft}|status={(int)ExampleStatus.Active})"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Status).Should().Contain([ExampleStatus.Draft, ExampleStatus.Active]);
        result.Items.Select(x => x.Status).Should().NotContain(ExampleStatus.Archived);
    }

    [Fact(DisplayName = "Handle: Should ignore invalid properties in the filter string")]
    public async Task Handle_InvalidFilterPropertiesProvided_IgnoresThem()
    {
        await fixture.Context.Set<Example>().AddAsync(new Example { Id = Guid.NewGuid(), Name = "Test" }, TestContext.Current.CancellationToken);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);
        var request = new GetExamplesV2.Request { Filter = "InvalidProp=Value" };

        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);
        result.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Handle: Should ignore invalid properties in the sort string")]
    public async Task Handle_InvalidSortPropertyProvided_ReturnsDefaultSort()
    {
        var baseName = $"V2InvalidSort_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_A" },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_B" }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);
        var request = new GetExamplesV2.Request { Filter = $"Name*{baseName}", Sort = "InvalidProp" };

        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);
        // Default sort is Name asc
        result.Items[0].Name.Should().Be($"{baseName}_A");
    }

    [Fact(DisplayName = "Handle: Should correctly handle special characters in search queries")]
    public async Task Handle_SpecialCharsUsedInSearch_ReturnsCorrectItems()
    {
        var specialName = "Test@#%^&*()";
        await fixture.Context.Set<Example>().AddAsync(new Example { Id = Guid.NewGuid(), Name = specialName }, TestContext.Current.CancellationToken);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);
        var request = new GetExamplesV2.Request
        {
            Search = "@#%",
            SearchField = new[] { "Name" }
        };

        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);
        result.Items.Should().Contain(x => x.Name == specialName);
    }

    [Fact(DisplayName = "Filter: Should work correctly when applied as a standalone extension method")]
    public async Task ApplyFilter_UsedStandalone_WorksCorrectly()
    {
        var baseName = $"V2Chain_{Guid.NewGuid()}";
        await fixture.Context.Set<Example>().AddAsync(new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Item", Price = 100 }, TestContext.Current.CancellationToken);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dbQuery = fixture.Context.Set<Example>().AsNoTracking();

        // Directly using the extension method from the split
        var result = await dbQuery
            .ApplyFilter(new FilterOptions { Filter = $"Name*{baseName}" })
            .ToListAsync(TestContext.Current.CancellationToken);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be($"{baseName}_Item");
    }

    [Fact(DisplayName = "Metadata: Request record should correctly implement the IPageOptions interface")]
    public void Request_Always_ImplementsIPageOptions()
    {
        var request = new GetExamplesV2.Request { Page = 5, PageSize = 50 };
        IPageOptions options = request;

        options.Page.Should().Be(5);
        options.PageSize.Should().Be(50);
    }

    [Fact(DisplayName = "Handle: Should filter examples by nested category properties")]
    public async Task Handle_NestedPropertyFilter_FiltersCorrectly()
    {
        // Arrange
        var cat1 = new ExampleCategory { Id = Guid.NewGuid(), Name = "NestedCat1" };
        var cat2 = new ExampleCategory { Id = Guid.NewGuid(), Name = "NestedCat2" };
        fixture.Context.Set<ExampleCategory>().AddRange(cat1, cat2);

        var baseName = $"V2Nested_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_1", Price = 10, CategoryId = cat1.Id },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_2", Price = 20, CategoryId = cat2.Id }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Filter by Category.Name
        var request = new GetExamplesV2.Request
        {
            Filter = $"Category.Name=NestedCat1"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].CategoryName.Should().Be("NestedCat1");
    }

    [Fact(DisplayName = "Handle: Should handle snake_case property names for nested filters correctly")]
    public async Task Handle_SnakeCaseInNestedFilters_HandlesCorrectly()
    {
        // Arrange
        var cat = new ExampleCategory { Id = Guid.NewGuid(), Name = "SnakeCaseCat" };
        fixture.Context.Set<ExampleCategory>().Add(cat);

        var baseName = $"V2SnakeNested_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().Add(new Example 
        { 
            Id = Guid.NewGuid(), 
            Name = $"{baseName}_Item", 
            CategoryId = cat.Id 
        });
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Filter by category.name (snake_case)
        var request = new GetExamplesV2.Request
        {
            Filter = "category.name=SnakeCaseCat"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().Contain(x => x.CategoryName == "SnakeCaseCat");
    }

    [Fact(DisplayName = "Handle: Should sort examples by nested category properties correctly")]
    public async Task Handle_NestedPropertySort_SortsCorrectly()
    {
        // Arrange
        var catA = new ExampleCategory { Id = Guid.NewGuid(), Name = "A_Category" };
        var catB = new ExampleCategory { Id = Guid.NewGuid(), Name = "B_Category" };
        fixture.Context.Set<ExampleCategory>().AddRange(catA, catB);

        var baseName = $"V2NestedSort_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_1", CategoryId = catB.Id },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_2", CategoryId = catA.Id }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Sort by Category.Name Ascending
        var request = new GetExamplesV2.Request
        {
            Filter = $"Name*{baseName}",
            Sort = "Category.Name"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items[0].CategoryName.Should().Be("A_Category");
        result.Items[1].CategoryName.Should().Be("B_Category");
    }

    [Fact(DisplayName = "Handle: Should match examples when searching within nested category properties")]
    public async Task Handle_NestedPropertySearch_SearchesCorrectly()
    {
        // Arrange
        var cat = new ExampleCategory { Id = Guid.NewGuid(), Name = "Special Category" };
        fixture.Context.Set<ExampleCategory>().Add(cat);

        var baseName = $"V2NestedSearch_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Match", CategoryId = cat.Id },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_NoMatch" }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(fixture.Context);

        // Act: Search for "Special" in Category.Name
        var request = new GetExamplesV2.Request
        {
            Search = "Special",
            SearchField = new[] { "Category.Name" }
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].CategoryName.Should().Be("Special Category");
    }
}
