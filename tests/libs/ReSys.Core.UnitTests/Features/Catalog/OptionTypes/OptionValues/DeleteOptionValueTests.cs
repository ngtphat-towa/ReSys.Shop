using Microsoft.EntityFrameworkCore;

using ErrorOr;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.DeleteOptionValue;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.OptionValues;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionValues")]
public class DeleteOptionValueTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "Handle: Should delete option value successfully")]
    public async Task Handle_ValidId_ShouldDelete()
    {
        // Arrange
        var ot = OptionType.Create("Material").Value;
        var ov = ot.AddValue("Silk").Value;
        fixture.Context.Set<OptionType>().Add(ot);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteOptionValue.Handler(fixture.Context);
        var command = new DeleteOptionValue.Command(ot.Id, ov.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);

        var exists = await fixture.Context.Set<OptionValue>().AnyAsync(x => x.Id == ov.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Handle: Should return NotFound when value does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new DeleteOptionValue.Handler(fixture.Context);
        var command = new DeleteOptionValue.Command(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
