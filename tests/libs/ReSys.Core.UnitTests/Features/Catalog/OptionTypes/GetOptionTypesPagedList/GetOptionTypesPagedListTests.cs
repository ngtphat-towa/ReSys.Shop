using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypesPagedList;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.GetOptionTypesPagedList;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class GetOptionTypesPagedListTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    static GetOptionTypesPagedListTests()
    {
        new OptionTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    public GetOptionTypesPagedListTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Handle: Should return paginated list of option types")]
    public async Task Handle_ValidRequest_ShouldReturnPagedList()
    {
        // Arrange
        var baseName = $"Paged_{Guid.NewGuid()}";
        for(int i=0; i<5; i++)
        {
            var ot = OptionType.Create($"{baseName}_{i}").Value;
            _fixture.Context.Set<OptionType>().Add(ot);
        }
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypesPagedList.GetOptionTypesPagedList.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypesPagedList.GetOptionTypesPagedList.Request 
        { 
            Filter = $"Name*{baseName}",
            Page = 1,
            PageSize = 2
        };
        var query = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypesPagedList.GetOptionTypesPagedList.Query(request);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
    }
}
