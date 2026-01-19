using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.GetPropertyTypeSelectList;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.PropertyTypes.GetPropertyTypeSelectList;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class GetPropertyTypeSelectListTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public GetPropertyTypeSelectListTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        new PropertyTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should return all items for select list when no paging provided")]
    public async Task Handle_NoPaging_ShouldReturnAll()
    {
        // Arrange
        var baseName = $"SelectPT_{Guid.NewGuid()}";
        for(int i=0; i<3; i++)
        {
            var pt = PropertyType.Create($"{baseName}_{i}").Value;
            _fixture.Context.Set<PropertyType>().Add(pt);
        }
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new Core.Features.Admin.Catalog.PropertyTypes.GetPropertyTypeSelectList.GetPropertyTypeSelectList.Handler(_fixture.Context);
        var request = new Core.Features.Admin.Catalog.PropertyTypes.GetPropertyTypeSelectList.GetPropertyTypeSelectList.Request 
        { 
            Filter = $"Name*{baseName}"
        };
        var query = new Core.Features.Admin.Catalog.PropertyTypes.GetPropertyTypeSelectList.GetPropertyTypeSelectList.Query(request);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(3);
    }
}
