using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeSelectList;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.GetOptionTypeSelectList;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class GetOptionTypeSelectListTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    static GetOptionTypeSelectListTests()
    {
        new OptionTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    public GetOptionTypeSelectListTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Handle: Should return all items for select list when no paging provided")]
    public async Task Handle_NoPaging_ShouldReturnAll()
    {
        // Arrange
        var baseName = $"Select_{Guid.NewGuid()}";
        for(int i=0; i<3; i++)
        {
            var ot = OptionType.Create($"{baseName}_{i}").Value;
            _fixture.Context.Set<OptionType>().Add(ot);
        }
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeSelectList.GetOptionTypeSelectList.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeSelectList.GetOptionTypeSelectList.Request 
        { 
            Filter = $"Name*{baseName}"
        };
        var query = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeSelectList.GetOptionTypeSelectList.Query(request);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(3);
        result.PageSize.Should().Be(3);
    }
}
