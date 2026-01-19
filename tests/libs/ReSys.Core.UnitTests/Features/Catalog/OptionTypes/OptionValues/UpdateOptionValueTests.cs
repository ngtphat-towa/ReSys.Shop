using Mapster;

using ErrorOr;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.UpdateOptionValue;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.OptionValues;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionValues")]
public class UpdateOptionValueTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public UpdateOptionValueTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        new OptionValueMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should update option value successfully")]
    public async Task Handle_ValidRequest_ShouldUpdate()
    {
        // Arrange
        var ot = OptionType.Create("Color").Value;
        var ov = ot.AddValue("Blue").Value;
        _fixture.Context.Set<OptionType>().Add(ot);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateOptionValue.Handler(_fixture.Context);
        var request = new UpdateOptionValue.Request
        {
            Name = "DarkBlue",
            Presentation = "Dark Blue",
            Position = 5
        };
        var command = new UpdateOptionValue.Command(ot.Id, ov.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("DarkBlue");
    }

    [Fact(DisplayName = "Handle: Should return error when updating to duplicate name within type")]
    public async Task Handle_DuplicateName_ShouldReturnError()
    {
        // Arrange
        var ot = OptionType.Create("Size").Value;
        var ov1 = ot.AddValue("Small").Value;
        var ov2 = ot.AddValue("Medium").Value;
        _fixture.Context.Set<OptionType>().Add(ot);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateOptionValue.Handler(_fixture.Context);
        var request = new UpdateOptionValue.Request { Name = "Medium" };
        var command = new UpdateOptionValue.Command(ot.Id, ov1.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionValueErrors.NameAlreadyExists("Medium"));
    }

    [Fact(DisplayName = "Handle: Should return NotFound when value does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new UpdateOptionValue.Handler(_fixture.Context);
        var request = new UpdateOptionValue.Request { Name = "Valid" };
        var command = new UpdateOptionValue.Command(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
