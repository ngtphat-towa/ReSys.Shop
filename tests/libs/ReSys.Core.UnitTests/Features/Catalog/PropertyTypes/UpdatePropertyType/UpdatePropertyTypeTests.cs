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
        var name = $"Old_{Guid.NewGuid()}";
        var pt = PropertyType.Create(name).Value;
        _fixture.Context.Set<PropertyType>().Add(pt);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Request 
        { 
            Name = $"New_{Guid.NewGuid()}",
            Presentation = "Updated",
            Kind = PropertyKind.Integer
        };
        var command = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Command(pt.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(request.Name);
    }

    [Fact(DisplayName = "Handle: Should return error when name is duplicate")]
    public async Task Handle_DuplicateName_ShouldReturnError()
    {
        // Arrange
        var name1 = $"Name1_{Guid.NewGuid()}";
        var name2 = $"Name2_{Guid.NewGuid()}";
        var pt1 = PropertyType.Create(name1).Value;
        var pt2 = PropertyType.Create(name2).Value;
        _fixture.Context.Set<PropertyType>().AddRange(pt1, pt2);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Request { Name = name2, Kind = PropertyKind.String };
        var command = new ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType.UpdatePropertyType.Command(pt1.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PropertyTypeErrors.DuplicateName);
    }
}
