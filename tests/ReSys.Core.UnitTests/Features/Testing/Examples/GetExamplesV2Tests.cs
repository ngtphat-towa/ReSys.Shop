using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Extensions;
using ReSys.Core.Common.Models;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.GetExamples;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Testing.Examples;

public class GetExamplesV2Tests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public GetExamplesV2Tests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "GetExamplesV2: Should filter by dynamic string")]
    public async Task Handle_WithFilter_ShouldReturnCorrectItems()
    {
        // Arrange
        var baseName = $"V2Filter_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Apple", Price = 10 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Banana", Price = 20 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Orange", Price = 30 }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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

    [Fact(DisplayName = "GetExamplesV2: Should sort by dynamic field")]
    public async Task Handle_WithSort_ShouldReturnSortedItems()
    {
        // Arrange
        var baseName = $"V2Sort_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_A", Price = 100 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_B", Price = 50 }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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

    [Fact(DisplayName = "GetExamplesV2: Should search globally")]
    public async Task Handle_WithSearch_ShouldMatchFields()
    {
        // Arrange
        var baseName = $"V2Search_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_FindMe", Description = "Test" },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_HideMe", Description = "Other" }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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

    [Fact(DisplayName = "GetExamplesV2: Should filter by Enum (Status)")]
    public async Task Handle_WithEnumFilter_ShouldReturnCorrectItems()
    {
        // Arrange
        var baseName = $"V2Enum_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Active", Status = ExampleStatus.Active },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Draft", Status = ExampleStatus.Draft }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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

    [Fact(DisplayName = "GetExamplesV2: Should filter by Array of IDs (manual OR group)")]
    public async Task Handle_WithIdArray_ShouldReturnCorrectItems()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = id1, Name = "Inc1" },
            new Example { Id = id2, Name = "Inc2" },
            new Example { Id = id3, Name = "Exc3" }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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

    [Fact(DisplayName = "GetExamplesV2: Should filter by multiple statuses using OR logic (snake_case)")]
    public async Task Handle_WithStatusOrLogic_ShouldWork()
    {
        // Arrange
        var baseName = $"V2StatusOr_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Draft", Status = ExampleStatus.Draft }, // 0
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Active", Status = ExampleStatus.Active }, // 1
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Archived", Status = ExampleStatus.Archived } // 2
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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

    [Fact(DisplayName = "GetExamplesV2: Should ignore invalid filter properties")]
    public async Task Handle_InvalidFilterProperty_ReturnsAll()
    {
        await _fixture.Context.Set<Example>().AddAsync(new Example { Id = Guid.NewGuid(), Name = "Test" }, TestContext.Current.CancellationToken);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);
        var request = new GetExamplesV2.Request { Filter = "InvalidProp=Value" };

        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);
        result.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "GetExamplesV2: Should ignore invalid sort properties")]
    public async Task Handle_InvalidSortProperty_ReturnsDefaultSort()
    {
        var baseName = $"V2InvalidSort_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_A" },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_B" }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);
        var request = new GetExamplesV2.Request { Filter = $"Name*{baseName}", Sort = "InvalidProp" };

        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);
        // Default sort is Name asc
        result.Items[0].Name.Should().Be($"{baseName}_A");
    }

    [Fact(DisplayName = "GetExamplesV2: Should handle special characters in search")]
    public async Task Handle_SpecialCharSearch_ReturnsCorrectItems()
    {
        var specialName = "Test@#%^&*()";
        await _fixture.Context.Set<Example>().AddAsync(new Example { Id = Guid.NewGuid(), Name = specialName }, TestContext.Current.CancellationToken);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);
        var request = new GetExamplesV2.Request
        {
            Search = "@#%",
            SearchField = new[] { "Name" }
        };

        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);
        result.Items.Should().Contain(x => x.Name == specialName);
    }

    [Fact(DisplayName = "GetExamplesV2: Should support standalone Filter chain")]
    public async Task Handle_StandaloneFilterChain_Works()
    {
        var baseName = $"V2Chain_{Guid.NewGuid()}";
        await _fixture.Context.Set<Example>().AddAsync(new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Item", Price = 100 }, TestContext.Current.CancellationToken);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dbQuery = _fixture.Context.Set<Example>().AsNoTracking();

        // Directly using the extension method from the split
        var result = await dbQuery
            .ApplyFilter(new FilterOptions { Filter = $"Name*{baseName}" })
            .ToListAsync(TestContext.Current.CancellationToken);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be($"{baseName}_Item");
    }

    [Fact(DisplayName = "GetExamplesV2: Request record should correctly implement IPageOptions")]
    public void Request_Implements_IPageOptions()
    {
        var request = new GetExamplesV2.Request { Page = 5, PageSize = 50 };
        IPageOptions options = request;

        options.Page.Should().Be(5);
        options.PageSize.Should().Be(50);
    }

    [Fact(DisplayName = "GetExamplesV2: Should filter by nested category name")]
    public async Task Handle_NestedFilter_ShouldWork()
    {
        // Arrange
        var cat1 = new ExampleCategory { Id = Guid.NewGuid(), Name = "NestedCat1" };
        var cat2 = new ExampleCategory { Id = Guid.NewGuid(), Name = "NestedCat2" };
        _fixture.Context.Set<ExampleCategory>().AddRange(cat1, cat2);

        var baseName = $"V2Nested_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_1", Price = 10, CategoryId = cat1.Id },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_2", Price = 20, CategoryId = cat2.Id }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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

    [Fact(DisplayName = "GetExamplesV2: Should filter by nested category name using snake_case")]
    public async Task Handle_NestedSnakeCaseFilter_ShouldWork()
    {
        // Arrange
        var cat = new ExampleCategory { Id = Guid.NewGuid(), Name = "SnakeCaseCat" };
        _fixture.Context.Set<ExampleCategory>().Add(cat);

        var baseName = $"V2SnakeNested_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().Add(new Example 
        { 
            Id = Guid.NewGuid(), 
            Name = $"{baseName}_Item", 
            CategoryId = cat.Id 
        });
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

        // Act: Filter by category.name (snake_case)
        var request = new GetExamplesV2.Request
        {
            Filter = "category.name=SnakeCaseCat"
        };
        var result = await handler.Handle(new GetExamplesV2.Query(request), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().Contain(x => x.CategoryName == "SnakeCaseCat");
    }

    [Fact(DisplayName = "GetExamplesV2: Should sort by nested category name")]
    public async Task Handle_NestedSort_ShouldWork()
    {
        // Arrange
        var catA = new ExampleCategory { Id = Guid.NewGuid(), Name = "A_Category" };
        var catB = new ExampleCategory { Id = Guid.NewGuid(), Name = "B_Category" };
        _fixture.Context.Set<ExampleCategory>().AddRange(catA, catB);

        var baseName = $"V2NestedSort_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_1", CategoryId = catB.Id },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_2", CategoryId = catA.Id }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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

    [Fact(DisplayName = "GetExamplesV2: Should search in nested category name")]
    public async Task Handle_NestedSearch_ShouldWork()
    {
        // Arrange
        var cat = new ExampleCategory { Id = Guid.NewGuid(), Name = "Special Category" };
        _fixture.Context.Set<ExampleCategory>().Add(cat);

        var baseName = $"V2NestedSearch_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Match", CategoryId = cat.Id },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_NoMatch" }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetExamplesV2.Handler(_fixture.Context);

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
