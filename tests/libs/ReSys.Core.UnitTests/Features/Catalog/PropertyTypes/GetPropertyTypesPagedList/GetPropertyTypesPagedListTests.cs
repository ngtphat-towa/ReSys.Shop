using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypesPagedList;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.PropertyTypes.GetPropertyTypesPagedList;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class GetPropertyTypesPagedListTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public GetPropertyTypesPagedListTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        new PropertyTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should return paged list of property types")]
    public async Task Handle_ValidRequest_ShouldReturnPagedList()
    {
        // Arrange
        var baseName = $"PagedPT_{Guid.NewGuid()}";
        for(int i=0; i<5; i++)
        {
            var pt = PropertyType.Create($"{baseName}_{i}").Value;
            _fixture.Context.Set<PropertyType>().Add(pt);
        }
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypesPagedList.GetPropertyTypesPagedList.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypesPagedList.GetPropertyTypesPagedList.Request 
        { 
            Filter = $"Name*{baseName}",
            PageSize = 2
        };
        var query = new ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypesPagedList.GetPropertyTypesPagedList.Query(request);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
    }
}
