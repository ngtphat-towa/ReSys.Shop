using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Extensions;
using ReSys.Core.Entities;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

namespace ReSys.Core.UnitTests.Common.Extensions;

public class QueryableExtensionsTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public QueryableExtensionsTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    #region ToPagedListAsync Tests

    [Fact(DisplayName = "ToPagedListAsync: Should return first page with default page size when both are null")]
    public async Task ToPagedListAsync_BothNull_ShouldReturnFirstPageWithDefaultSize()
    {
        // Arrange
        var baseName = $"PagedNull_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: null,
            pageSize: null);

        // Assert
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10); // Default page size
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "ToPagedListAsync: Should return first page with default size when page is null")]
    public async Task ToPagedListAsync_PageNull_ShouldReturnFirstPageWithProvidedSize()
    {
        // Arrange
        var baseName = $"PageNull_{Guid.NewGuid()}";
        await SeedExamples(baseName, 10);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: null,
            pageSize: 5);

        // Assert
        result.Page.Should().Be(1); // Default page
        result.PageSize.Should().Be(5);
        result.Items.Should().HaveCount(5);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should return requested page with default size when pageSize is null")]
    public async Task ToPagedListAsync_PageSizeNull_ShouldReturnRequestedPageWithDefaultSize()
    {
        // Arrange
        var baseName = $"SizeNull_{Guid.NewGuid()}";
        await SeedExamples(baseName, 25);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: 2,
            pageSize: null);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10); // Default page size
        result.Items.Should().HaveCount(10);
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact(DisplayName = "ToPagedListAsync: Should use default page when page is zero")]
    public async Task ToPagedListAsync_PageZero_ShouldFallbackToDefaultPage()
    {
        // Arrange
        var baseName = $"PageZero_{Guid.NewGuid()}";
        await SeedExamples(baseName, 5);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: 0,
            pageSize: 3);

        // Assert
        result.Page.Should().Be(1); // Fallback to default
        result.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should use default page when page is negative")]
    public async Task ToPagedListAsync_PageNegative_ShouldFallbackToDefaultPage()
    {
        // Arrange
        var baseName = $"PageNeg_{Guid.NewGuid()}";
        await SeedExamples(baseName, 5);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: -5,
            pageSize: 3);

        // Assert
        result.Page.Should().Be(1);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should use default pageSize when pageSize is zero")]
    public async Task ToPagedListAsync_PageSizeZero_ShouldFallbackToDefaultSize()
    {
        // Arrange
        var baseName = $"SizeZero_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: 1,
            pageSize: 0);

        // Assert
        result.PageSize.Should().Be(10); // Fallback to default
        result.Items.Should().HaveCount(10);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should use default pageSize when pageSize is negative")]
    public async Task ToPagedListAsync_PageSizeNegative_ShouldFallbackToDefaultSize()
    {
        // Arrange
        var baseName = $"SizeNeg_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: 1,
            pageSize: -10);

        // Assert
        result.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should return correct items for specific page")]
    public async Task ToPagedListAsync_SpecificPage_ShouldReturnCorrectItems()
    {
        // Arrange
        var baseName = $"SpecificPage_{Guid.NewGuid()}";
        await SeedExamples(baseName, 10);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName))
            .OrderBy(p => p.Name);

        // Act - Get second page with 3 items per page
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: 2,
            pageSize: 3);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(10);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "ToPagedListAsync: Should return empty list when page exceeds total pages")]
    public async Task ToPagedListAsync_PageExceedsTotalPages_ShouldReturnEmptyList()
    {
        // Arrange
        var baseName = $"ExceedPage_{Guid.NewGuid()}";
        await SeedExamples(baseName, 5);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { p.Id, p.Name },
            page: 100,
            pageSize: 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.HasNextPage.Should().BeFalse();
    }

    [Fact(DisplayName = "ToPagedListAsync: Should correctly project to destination type")]
    public async Task ToPagedListAsync_WithProjection_ShouldReturnProjectedItems()
    {
        // Arrange
        var baseName = $"Projection_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().Add(
            new Example 
            { 
                Id = Guid.NewGuid(), 
                Name = $"{baseName}_Item", 
                Price = 99.99m,
                Description = "Test description"
            });
        await _fixture.Context.SaveChangesAsync(CancellationToken.None);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedListAsync(
            p => new { FullName = p.Name, Cost = p.Price },
            page: 1,
            pageSize: 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].FullName.Should().Contain(baseName);
        result.Items[0].Cost.Should().Be(99.99m);
    }

    #endregion

    #region ToPagedOrAllAsync Tests

    [Fact(DisplayName = "ToPagedOrAllAsync: Should return ALL items when both page and pageSize are null")]
    public async Task ToPagedOrAllAsync_BothNull_ShouldReturnAllItems()
    {
        // Arrange
        var baseName = $"AllItems_{Guid.NewGuid()}";
        await SeedExamples(baseName, 25);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedOrAllAsync(
            p => new { p.Id, p.Name },
            page: null,
            pageSize: null);

        // Assert
        result.Items.Should().HaveCount(25); // ALL items returned
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(25); // PageSize equals total count
        result.HasNextPage.Should().BeFalse();
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Should return paged result when only page is provided")]
    public async Task ToPagedOrAllAsync_OnlyPageProvided_ShouldReturnPagedResult()
    {
        // Arrange
        var baseName = $"OnlyPage_{Guid.NewGuid()}";
        await SeedExamples(baseName, 20);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedOrAllAsync(
            p => new { p.Id, p.Name },
            page: 1,
            pageSize: null);

        // Assert
        result.Items.Should().HaveCount(10); // Default page size applied
        result.TotalCount.Should().Be(20);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Should return paged result when only pageSize is provided")]
    public async Task ToPagedOrAllAsync_OnlyPageSizeProvided_ShouldReturnPagedResult()
    {
        // Arrange
        var baseName = $"OnlySize_{Guid.NewGuid()}";
        await SeedExamples(baseName, 20);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedOrAllAsync(
            p => new { p.Id, p.Name },
            page: null,
            pageSize: 5);

        // Assert
        result.Page.Should().Be(1); // Default page
        result.PageSize.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(20);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Should return paged result when both page and pageSize are provided")]
    public async Task ToPagedOrAllAsync_BothProvided_ShouldReturnPagedResult()
    {
        // Arrange
        var baseName = $"BothProvided_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedOrAllAsync(
            p => new { p.Id, p.Name },
            page: 2,
            pageSize: 5);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Should return empty PagedList when no items match query and both params null")]
    public async Task ToPagedOrAllAsync_NoMatchingItems_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentName = $"NonExistent_{Guid.NewGuid()}";
        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(nonExistentName));

        // Act
        var result = await query.ToPagedOrAllAsync(
            p => new { p.Id, p.Name },
            page: null,
            pageSize: null);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(0); // Empty list has pageSize = count = 0
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Should correctly project all items to destination type")]
    public async Task ToPagedOrAllAsync_AllWithProjection_ShouldReturnAllProjectedItems()
    {
        // Arrange
        var baseName = $"AllProjected_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_1", Price = 10m },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_2", Price = 20m },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_3", Price = 30m }
        );
        await _fixture.Context.SaveChangesAsync(CancellationToken.None);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName))
            .OrderBy(p => p.Price);

        // Act
        var result = await query.ToPagedOrAllAsync(
            p => new { ExampleName = p.Name, ExamplePrice = p.Price },
            page: null,
            pageSize: null);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].ExamplePrice.Should().Be(10m);
        result.Items[1].ExamplePrice.Should().Be(20m);
        result.Items[2].ExamplePrice.Should().Be(30m);
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Should handle large dataset when returning all items")]
    public async Task ToPagedOrAllAsync_LargeDataset_ShouldReturnAllItems()
    {
        // Arrange
        var baseName = $"LargeData_{Guid.NewGuid()}";
        await SeedExamples(baseName, 100);

        var query = _fixture.Context.Set<Example>()
            .Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedOrAllAsync(
            p => new { p.Id, p.Name },
            page: null,
            pageSize: null);

        // Assert
        result.Items.Should().HaveCount(100);
        result.TotalCount.Should().Be(100);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private async Task SeedExamples(string baseName, int count)
    {
        var Examples = Enumerable.Range(1, count)
            .Select(i => new Example
            {
                Id = Guid.NewGuid(),
                Name = $"{baseName}_{i:D3}",
                Price = i * 10
            })
            .ToList();

        _fixture.Context.Set<Example>().AddRange(Examples);
        await _fixture.Context.SaveChangesAsync(CancellationToken.None);
    }

    #endregion
}
