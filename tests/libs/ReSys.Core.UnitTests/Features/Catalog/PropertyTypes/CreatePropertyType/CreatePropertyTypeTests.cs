using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Catalog.PropertyTypes.CreatePropertyType;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.PropertyTypes.CreatePropertyType;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class CreatePropertyTypeTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public CreatePropertyTypeTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        new PropertyTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should create property type successfully")]
    public async Task Handle_ValidRequest_ShouldCreate()
    {
        // Arrange
        var handler = new ReSys.Core.Features.Catalog.PropertyTypes.CreatePropertyType.CreatePropertyType.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.PropertyTypes.CreatePropertyType.CreatePropertyType.Request 
        { 
            Name = $"Brand_{Guid.NewGuid()}",
            Presentation = "Brand",
            Kind = PropertyKind.String
        };
        var command = new ReSys.Core.Features.Catalog.PropertyTypes.CreatePropertyType.CreatePropertyType.Command(request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(request.Name);
    }
}