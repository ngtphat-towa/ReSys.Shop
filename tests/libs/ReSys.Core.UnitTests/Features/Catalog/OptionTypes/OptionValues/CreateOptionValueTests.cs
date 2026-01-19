using Microsoft.EntityFrameworkCore;

using ErrorOr;

using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.CreateOptionValue;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.OptionValues;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionValues")]
public class CreateOptionValueTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public CreateOptionValueTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        new OptionValueMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should create option value through root successfully")]
    public async Task Handle_ValidRequest_ShouldCreate()
    {
        // Arrange
        var ot = OptionType.Create("Material").Value;
        _fixture.Context.Set<OptionType>().Add(ot);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateOptionValue.Handler(_fixture.Context);
        var request = new CreateOptionValue.Request
        {
            Name = "Cotton",
            Presentation = "Natural Cotton"
        };
        var command = new CreateOptionValue.Command(ot.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Cotton");

        var exists = await _fixture.Context.Set<OptionValue>().AnyAsync(x => x.Id == result.Value.Id, TestContext.Current.CancellationToken);
        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should return error when name is duplicate within same type")]
    public async Task Handle_DuplicateName_ShouldReturnError()
    {
        // Arrange
        var ot = OptionType.Create("Size").Value;
        ot.AddValue("Large");
        _fixture.Context.Set<OptionType>().Add(ot);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateOptionValue.Handler(_fixture.Context);
        var request = new CreateOptionValue.Request { Name = "Large" };
        var command = new CreateOptionValue.Command(ot.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionValueErrors.NameAlreadyExists("Large"));
    }

    [Fact(DisplayName = "Handle: Should return NotFound when parent option type does not exist")]
    public async Task Handle_ParentNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new CreateOptionValue.Handler(_fixture.Context);
        var request = new CreateOptionValue.Request { Name = "Valid" };
        var command = new CreateOptionValue.Command(Guid.NewGuid(), request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
