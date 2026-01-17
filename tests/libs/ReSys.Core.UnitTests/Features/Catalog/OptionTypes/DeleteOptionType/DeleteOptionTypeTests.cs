using Microsoft.EntityFrameworkCore;
using ErrorOr;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.DeleteOptionType;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.DeleteOptionType;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class DeleteOptionTypeTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "Handle: Should return error when deleting option type with values")]
    public async Task Handle_WithValues_ShouldReturnError()
    {
        // Arrange
        var optionType = OptionType.Create("Material").Value;
        optionType.AddValue("Cotton");
        
        fixture.Context.Set<OptionType>().Add(optionType);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.OptionTypes.DeleteOptionType.DeleteOptionType.Handler(fixture.Context);
        var command = new ReSys.Core.Features.Catalog.OptionTypes.DeleteOptionType.DeleteOptionType.Command(optionType.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionTypeErrors.CannotDeleteWithValues);
        
        // Verify it still exists in DB
        var exists = await fixture.Context.Set<OptionType>().AnyAsync(x => x.Id == optionType.Id, TestContext.Current.CancellationToken);
        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should delete empty option type successfully")]
    public async Task Handle_WithoutValues_ShouldSucceed()
    {
        // Arrange
        var optionType = OptionType.Create("Size").Value;
        
        fixture.Context.Set<OptionType>().Add(optionType);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.OptionTypes.DeleteOptionType.DeleteOptionType.Handler(fixture.Context);
        var command = new ReSys.Core.Features.Catalog.OptionTypes.DeleteOptionType.DeleteOptionType.Command(optionType.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        
        // Verify it's gone from DB
        var exists = await fixture.Context.Set<OptionType>().AnyAsync(x => x.Id == optionType.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Handle: Should return NotFound when option type does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new ReSys.Core.Features.Catalog.OptionTypes.DeleteOptionType.DeleteOptionType.Handler(fixture.Context);
        var command = new ReSys.Core.Features.Catalog.OptionTypes.DeleteOptionType.DeleteOptionType.Command(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}