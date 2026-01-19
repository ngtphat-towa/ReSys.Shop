using Microsoft.EntityFrameworkCore;

using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.CreateOptionType;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class CreateOptionTypeTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    static CreateOptionTypeTests()
    {
        new OptionTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
        new Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common.OptionValueMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    public CreateOptionTypeTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Handle: Should create option type successfully")]
    public async Task Handle_ValidRequest_ShouldCreateOptionType()
    {
        // Arrange
        var handler = new CreateOptionType.Handler(_fixture.Context);
        var request = new CreateOptionType.Request
        {
            Name = $"Material_{Guid.NewGuid()}",
            Presentation = "Material Type",
            Position = 1,
            Filterable = true
        };
        var command = new CreateOptionType.Command(request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(request.Name);

        var exists = await _fixture.Context.Set<OptionType>().AnyAsync(x => x.Id == result.Value.Id, TestContext.Current.CancellationToken);
        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should return error when name is duplicate")]
    public async Task Handle_DuplicateName_ShouldReturnError()
    {
        // Arrange
        var name = $"Duplicate_{Guid.NewGuid()}";
        var existing = OptionType.Create(name).Value;
        _fixture.Context.Set<OptionType>().Add(existing);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateOptionType.Handler(_fixture.Context);
        var request = new CreateOptionType.Request { Name = name };
        var command = new CreateOptionType.Command(request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionTypeErrors.DuplicateName);
    }
}
