using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.GetOptionTypesPagedList;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class GetOptionTypesPagedListTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture = fixture;

    static GetOptionTypesPagedListTests()
    {
        new OptionTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should return paginated list of option types")]
    public async Task Handle_ValidRequest_ShouldReturnPagedList()
    {
        // Arrange
        var baseName = $"Paged_{Guid.NewGuid()}";
        for (int i = 0; i < 5; i++)
        {
            var ot = OptionType.Create($"{baseName}_{i}").Value;
            _fixture.Context.Set<OptionType>().Add(ot);
        }
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOptionTypesPagedList.Handler(_fixture.Context);
        var request = new GetOptionTypesPagedList.Request
        {
            Filter = $"Name*{baseName}",
            Page = 1,
            PageSize = 2
        };
        var query = new GetOptionTypesPagedList.Query(request);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact(DisplayName = "Handle: Should filter by search text")]
    public async Task Handle_Search_ShouldReturnFiltered()
    {
        // Arrange
        var uniqueSearch = $"Search_{Guid.NewGuid()}";
        var ot1 = OptionType.Create(uniqueSearch).Value;
        var ot2 = OptionType.Create("Other").Value;
        _fixture.Context.Set<OptionType>().AddRange(ot1, ot2);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOptionTypesPagedList.Handler(_fixture.Context);
        var request = new GetOptionTypesPagedList.Request
        {
            Search = uniqueSearch
        };
        var query = new GetOptionTypesPagedList.Query(request);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().ContainSingle(x => x.Name == uniqueSearch);
    }
}
