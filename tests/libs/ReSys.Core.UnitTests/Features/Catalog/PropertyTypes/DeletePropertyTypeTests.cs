using Microsoft.EntityFrameworkCore;
using ErrorOr;

using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.DeletePropertyType;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.PropertyTypes.DeletePropertyType;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class DeletePropertyTypeTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "Handle: Should delete property type successfully")]
    public async Task Handle_ValidId_ShouldDelete()
    {
        // Arrange
        var pt = PropertyType.Create("DeleteMe").Value;
        fixture.Context.Set<PropertyType>().Add(pt);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new Core.Features.Admin.Catalog.PropertyTypes.DeletePropertyType.DeletePropertyType.Handler(fixture.Context);
        var command = new Core.Features.Admin.Catalog.PropertyTypes.DeletePropertyType.DeletePropertyType.Command(pt.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        
        var exists = await fixture.Context.Set<PropertyType>().AnyAsync(x => x.Id == pt.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }
}
