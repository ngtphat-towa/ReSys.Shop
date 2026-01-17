using Mapster;

using ErrorOr;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.UpdateOptionType;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class UpdateOptionTypeTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    static UpdateOptionTypeTests()
    {
        new OptionTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    public UpdateOptionTypeTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Handle: Should update option type successfully")]
    public async Task Handle_ValidRequest_ShouldUpdate()
    {
        // Arrange
        var optionType = OptionType.Create("OldName").Value;
        _fixture.Context.Set<OptionType>().Add(optionType);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateOptionType.Handler(_fixture.Context);
        var request = new UpdateOptionType.Request
        {
            Name = "NewName",
            Presentation = "Updated",
            Position = 10,
            Filterable = true
        };
        var command = new UpdateOptionType.Command(optionType.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("NewName");
        result.Value.Presentation.Should().Be("Updated");
    }

    [Fact(DisplayName = "Handle: Should return NotFound when option type does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new UpdateOptionType.Handler(_fixture.Context);
        var command = new UpdateOptionType.Command(
            Guid.NewGuid(),
            new UpdateOptionType.Request { Name = "Valid" });

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact(DisplayName = "Handle: Should return error when updating to duplicate name")]
    public async Task Handle_DuplicateName_ShouldReturnError()
    {
        // Arrange
        var name1 = $"Name1_{Guid.NewGuid()}";
        var name2 = $"Name2_{Guid.NewGuid()}";

        var ot1 = OptionType.Create(name1).Value;
        var ot2 = OptionType.Create(name2).Value;
        _fixture.Context.Set<OptionType>().AddRange(ot1, ot2);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateOptionType.Handler(_fixture.Context);
        var request = new UpdateOptionType.Request { Name = name2 };
        var command = new UpdateOptionType.Command(ot1.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionTypeErrors.DuplicateName);
    }
}
