using Microsoft.EntityFrameworkCore;
using Mapster;
using ErrorOr;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue;

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

        var handler = new ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue.UpdateOptionValue.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue.UpdateOptionValue.Request 
        { 
            Name = "DarkBlue",
            Presentation = "Dark Blue",
            Position = 5
        };
        var command = new ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue.UpdateOptionValue.Command(ot.Id, ov.Id, request);

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

        var handler = new ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue.UpdateOptionValue.Handler(_fixture.Context);
        var request = new ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue.UpdateOptionValue.Request { Name = "Medium" };
        var command = new ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue.UpdateOptionValue.Command(ot.Id, ov1.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionValueErrors.NameAlreadyExists("Medium"));
    }
}
