using Microsoft.EntityFrameworkCore;
using Mapster;
using ErrorOr;

using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.PropertyTypes.UpdatePropertyType;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class UpdatePropertyTypeTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public UpdatePropertyTypeTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        new PropertyTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should update property type successfully")]
    public async Task Handle_ValidRequest_ShouldUpdate()
    {
        // Arrange
        var pt = PropertyType.Create("Old").Value;
        _fixture.Context.Set<PropertyType>().Add(pt);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Request 
        { 
            Name = "New",
            Presentation = "Updated",
            Kind = PropertyKind.Integer
        };
        var command = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Command(pt.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("New");
    }
}
