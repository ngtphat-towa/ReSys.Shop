using Mapster;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Query")]
public class MapsterPaginationExtensionsTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "ToPagedListAsync (Mapster): Should return paged result using Mapster projection")]
    public async Task ToPagedListAsync_Mapster_Should_ReturnPagedResult()
    {
        // Arrange
        var baseName = $"MapsterPaged_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));
        var options = new QueryOptions { Page = 2, PageSize = 5 };

        // Act
        var result = await query.ToPagedListAsync<Example, ExampleTestDto>(options, TestContext.Current.CancellationToken);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.Items[0].Should().BeOfType<ExampleTestDto>();
    }

    [Fact(DisplayName = "ToPagedOrAllAsync (Mapster): Should return all items when options are null")]
    public async Task ToPagedOrAllAsync_Mapster_Should_ReturnAll_WhenOptionsNull()
    {
        // Arrange
        var baseName = $"MapsterAll_{Guid.NewGuid()}";
        await SeedExamples(baseName, 10);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedOrAllAsync<Example, ExampleTestDto>(null, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(10);
    }

    [Fact(DisplayName = "ToPagedOrEmptyAsync (Mapster): Should return empty result when options are null")]
    public async Task ToPagedOrEmptyAsync_Mapster_Should_ReturnEmpty_WhenOptionsNull()
    {
        // Arrange
        var baseName = $"MapsterEmpty_{Guid.NewGuid()}";
        await SeedExamples(baseName, 5);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        // Act
        var result = await query.ToPagedOrEmptyAsync<Example, ExampleTestDto>(null, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

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

        fixture.Context.Set<Example>().AddRange(Examples);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public record ExampleTestDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
