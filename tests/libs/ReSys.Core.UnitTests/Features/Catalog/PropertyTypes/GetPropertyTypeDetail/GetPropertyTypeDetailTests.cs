using Microsoft.EntityFrameworkCore;
using Mapster;
using ErrorOr;

using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypeDetail;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.PropertyTypes.GetPropertyTypeDetail;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class GetPropertyTypeDetailTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public GetPropertyTypeDetailTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        new PropertyTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should return detail for existing property type")]
    public async Task Handle_Existing_ShouldReturnDetail()
    {
        // Arrange
        var pt = PropertyType.Create("Detail").Value;
        _fixture.Context.Set<PropertyType>().Add(pt);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypeDetail.GetPropertyTypeDetail.Handler(_fixture.Context);
        var query = new ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypeDetail.GetPropertyTypeDetail.Query(
            new ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypeDetail.GetPropertyTypeDetail.Request(pt.Id));

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(pt.Id);
    }
}