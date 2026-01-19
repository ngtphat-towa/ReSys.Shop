using Microsoft.EntityFrameworkCore;

using Mapster;

using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.Common;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.PropertyTypes.Common;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class PropertyTypeMappingTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly TypeAdapterConfig _config;

    public PropertyTypeMappingTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _config = new TypeAdapterConfig();
        new PropertyTypeMappings().Register(_config);
    }

    [Fact(DisplayName = "Projection: PropertyTypeListItem should map correctly")]
    public async Task Projection_PropertyTypeListItem_ShouldMapCorrectly()
    {
        // Arrange
        var name = $"Weight_{Guid.NewGuid()}";
        var pt = PropertyType.Create(name, "Product Weight", PropertyKind.Float).Value;
        _fixture.Context.Set<PropertyType>().Add(pt);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _fixture.Context.Set<PropertyType>()
            .Where(x => x.Id == pt.Id)
            .ProjectToType<PropertyTypeListItem>(_config)
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(pt.Id);
        result.Name.Should().Be(name);
        result.Kind.Should().Be(PropertyKind.Float);
    }
}