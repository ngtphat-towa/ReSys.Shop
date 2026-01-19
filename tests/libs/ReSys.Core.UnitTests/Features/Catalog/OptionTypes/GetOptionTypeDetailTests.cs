using Mapster;

using ErrorOr;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.GetOptionTypeDetail;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class GetOptionTypeDetailTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture = fixture;

    static GetOptionTypeDetailTests()
    {
        new OptionTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
        new OptionValueMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Handle: Should return full detail for existing option type")]
    public async Task Handle_Existing_ShouldReturnDetail()
    {
        // Arrange
        var optionType = OptionType.Create("DetailTest").Value;
        optionType.AddValue("V1");
        
        _fixture.Context.Set<OptionType>().Add(optionType);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOptionTypeDetail.Handler(_fixture.Context);
        var query = new GetOptionTypeDetail.Query(
            new GetOptionTypeDetail.Request(optionType.Id));

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(optionType.Id);
        result.Value.OptionValues.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handle: Should return values sorted by position")]
    public async Task Handle_Existing_ShouldReturnSortedValues()
    {
        // Arrange
        var optionType = OptionType.Create("SortedValues").Value;
        optionType.AddValue("V2", "V2"); // pos 0
        optionType.AddValue("V1", "V1"); // pos 1
        
        // Manual override positions to test sorting
        var values = optionType.OptionValues.ToList();
        values[0].Update("V2", "V2", 10);
        values[1].Update("V1", "V1", 5);

        _fixture.Context.Set<OptionType>().Add(optionType);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOptionTypeDetail.Handler(_fixture.Context);
        var query = new GetOptionTypeDetail.Query(
            new GetOptionTypeDetail.Request(optionType.Id));

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.OptionValues.Should().HaveCount(2);
        result.Value.OptionValues.First().Name.Should().Be("V1"); // Pos 5 comes before Pos 10
    }

    [Fact(DisplayName = "Handle: Should return NotFound for non-existent option type")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new GetOptionTypeDetail.Handler(_fixture.Context);
        var query = new GetOptionTypeDetail.Query(
            new GetOptionTypeDetail.Request(Guid.NewGuid()));

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
