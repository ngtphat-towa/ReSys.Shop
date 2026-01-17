using Microsoft.EntityFrameworkCore;
using Mapster;
using ErrorOr;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeDetail;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.GetOptionTypeDetail;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class GetOptionTypeDetailTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    static GetOptionTypeDetailTests()
    {
        new OptionTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    public GetOptionTypeDetailTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Handle: Should return full detail for existing option type")]
    public async Task Handle_Existing_ShouldReturnDetail()
    {
        // Arrange
        var optionType = OptionType.Create("DetailTest").Value;
        optionType.AddValue("V1");
        
        _fixture.Context.Set<OptionType>().Add(optionType);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeDetail.GetOptionTypeDetail.Handler(_fixture.Context);
        var query = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeDetail.GetOptionTypeDetail.Query(
            new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeDetail.GetOptionTypeDetail.Request(optionType.Id));

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(optionType.Id);
        result.Value.OptionValues.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handle: Should return NotFound for non-existent option type")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeDetail.GetOptionTypeDetail.Handler(_fixture.Context);
        var query = new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeDetail.GetOptionTypeDetail.Query(
            new ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeDetail.GetOptionTypeDetail.Request(Guid.NewGuid()));

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
