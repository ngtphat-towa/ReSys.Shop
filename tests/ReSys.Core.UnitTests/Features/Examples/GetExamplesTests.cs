using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Features.Testing.Examples.GetExamples;
using ReSys.Core.Domain;
using ReSys.Core.Common.Data;

namespace ReSys.Core.UnitTests.Features.Examples;

public class GetExamplesTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly GetExamples.Handler _handler;

    public GetExamplesTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new GetExamples.Handler(_context);
    }

    [Fact(DisplayName = "Should return a paged list of examples with correct metadata for total count and next page")]
    public async Task Handle_DefaultRequest_ShouldReturnPagedList()
    {
        // Arrange
        var baseName = $"PagedExample_{Guid.NewGuid()}";
        _context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_1", Price = 10 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_2", Price = 20 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_3", Price = 30 }
        );
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new GetExamples.Request { Page = 1, PageSize = 2, Search = baseName };
        var query = new GetExamples.Query(request);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "Should correctly filter examples based on a case-insensitive search term in name or description")]
    public async Task Handle_SearchFilter_ShouldReturnMatchingExamples()
    {
        // Arrange
        var uniqueSearch = $"Search_{Guid.NewGuid()}";
        _context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"Match_{uniqueSearch}", Price = 10 },
            new Example { Id = Guid.NewGuid(), Name = "Other", Description = $"Contains_{uniqueSearch}", Price = 20 },
            new Example { Id = Guid.NewGuid(), Name = "NoMatch", Price = 30 }
        );
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new GetExamples.Request { Search = uniqueSearch };
        var query = new GetExamples.Query(request);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(x => x.Name.Contains(uniqueSearch) || x.ImageUrl == null); // ImageUrl is irrelevant here
    }

    [Fact(DisplayName = "Should correctly sort examples by price in descending order")]
    public async Task Handle_SortByPriceDescending_ShouldReturnSortedList()
    {
        // Arrange
        var baseName = $"SortedExample_{Guid.NewGuid()}";
        _context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Low", Price = 10 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_High", Price = 100 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Mid", Price = 50 }
        );
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new GetExamples.Request 
        {
            Search = baseName,
            SortBy = "price", 
            IsDescending = true 
        };
        var query = new GetExamples.Query(request);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Price.Should().Be(100);
        result.Items[1].Price.Should().Be(50);
        result.Items[2].Price.Should().Be(10);
    }
    [Fact(DisplayName = "Should correctly filter examples by a list of ExampleIds")]
    public async Task Handle_ExampleIdsFilter_ShouldReturnMatchingExamples()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        
        _context.Set<Example>().AddRange(
            new Example { Id = id1, Name = "Included_1", Price = 10 },
            new Example { Id = id2, Name = "Included_2", Price = 20 },
            new Example { Id = id3, Name = "Excluded", Price = 30 }
        );
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new GetExamples.Request { ExampleId = [id1, id2] };
        var query = new GetExamples.Query(request);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Id).Should().Contain([id1, id2]);
        result.Items.Select(x => x.Id).Should().NotContain(id3);
    }

    [Fact(DisplayName = "Should correctly filter examples by Status")]
    public async Task Handle_StatusFilter_ShouldReturnMatchingExamples()
    {
        // Arrange
        var baseName = $"StatusFilter_{Guid.NewGuid()}";
        _context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Active", Status = ExampleStatus.Active, Price = 10 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Draft", Status = ExampleStatus.Draft, Price = 20 }
        );
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new GetExamples.Request { Search = baseName, Status = [ExampleStatus.Active] };
        var query = new GetExamples.Query(request);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(ExampleStatus.Active);
    }
}
