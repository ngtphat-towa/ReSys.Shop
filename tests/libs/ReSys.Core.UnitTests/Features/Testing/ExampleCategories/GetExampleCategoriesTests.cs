using ReSys.Core.Domain.Testing.ExampleCategories;
using ReSys.Core.Features.Testing.ExampleCategories.GetExampleCategories;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Testing.ExampleCategories;

public class GetExampleCategoriesTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "GetExampleCategories: Should filter by name")]
    public async Task Handle_WithFilter_ShouldReturnCorrectItems()
    {
        // Arrange
        var baseName = $"CatFilter_{Guid.NewGuid()}";
        fixture.Context.Set<ExampleCategory>().AddRange(
            new ExampleCategory { Id = Guid.NewGuid(), Name = $"{baseName}_Apple" },
            new ExampleCategory { Id = Guid.NewGuid(), Name = $"{baseName}_Banana" }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExampleCategories.Handler(fixture.Context);

        // Act
        var request = new GetExampleCategories.Request 
        { 
            Filter = $"Name*{baseName},Name*Apple" 
        };
        var result = await handler.Handle(new GetExampleCategories.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Contain("Apple");
    }

    [Fact(DisplayName = "GetExampleCategories: Should sort by name descending")]
    public async Task Handle_WithSort_ShouldReturnSortedItems()
    {
        // Arrange
        var baseName = $"CatSort_{Guid.NewGuid()}";
        fixture.Context.Set<ExampleCategory>().AddRange(
            new ExampleCategory { Id = Guid.NewGuid(), Name = $"{baseName}_A" },
            new ExampleCategory { Id = Guid.NewGuid(), Name = $"{baseName}_B" }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExampleCategories.Handler(fixture.Context);

        // Act
        var request = new GetExampleCategories.Request 
        { 
            Filter = $"Name*{baseName}",
            Sort = "Name desc" 
        };
        var result = await handler.Handle(new GetExampleCategories.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items[0].Name.Should().Be($"{baseName}_B");
        result.Items[1].Name.Should().Be($"{baseName}_A");
    }

    [Fact(DisplayName = "GetExampleCategories: Should search globally")]
    public async Task Handle_WithSearch_ShouldMatchFields()
    {
        // Arrange
        var baseName = $"CatSearch_{Guid.NewGuid()}";
        fixture.Context.Set<ExampleCategory>().AddRange(
            new ExampleCategory { Id = Guid.NewGuid(), Name = $"{baseName}_FindMe", Description = "Test" },
            new ExampleCategory { Id = Guid.NewGuid(), Name = $"{baseName}_HideMe", Description = "Other" }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExampleCategories.Handler(fixture.Context);

        // Act
        var request = new GetExampleCategories.Request 
        { 
            Search = "FindMe",
            SearchField = new[] { "Name", "Description" }
        };
        var result = await handler.Handle(new GetExampleCategories.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Contain("FindMe");
    }

    [Fact(DisplayName = "GetExampleCategories: Should return empty page when no matches")]
    public async Task Handle_NoMatches_ShouldReturnEmpty()
    {
        var uniqueSearch = $"NonExistent_{Guid.NewGuid()}";
        var request = new GetExampleCategories.Request 
        { 
            Search = uniqueSearch,
            SearchField = new[] { "Name" } 
        };
        var result = await new GetExampleCategories.Handler(fixture.Context).Handle(new GetExampleCategories.Query(request), TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "GetExampleCategories: Should handle pagination correctly")]
    public async Task Handle_Pagination_Works()
    {
        // Arrange
        var baseName = $"CatPage_{Guid.NewGuid()}";
        for (int i = 1; i <= 5; i++)
        {
            fixture.Context.Set<ExampleCategory>().Add(new ExampleCategory { Id = Guid.NewGuid(), Name = $"{baseName}_{i}" });
        }
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExampleCategories.Handler(fixture.Context);

        // Act: Page 2, Size 2
        var request = new GetExampleCategories.Request 
        { 
            Filter = $"Name*{baseName}",
            Sort = "Name",
            Page = 2,
            PageSize = 2
        };
        var result = await handler.Handle(new GetExampleCategories.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(2);
        result.Items[0].Name.Should().Be($"{baseName}_3");
    }
}
