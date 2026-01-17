using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.OptionValues.Common;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionValues")]
public class OptionValueMappingTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public OptionValueMappingTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        new OptionValueMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Projection: OptionValueModel should map correctly")]
    public async Task Projection_OptionValueModel_ShouldMapCorrectly()
    {
        // Arrange
        var ot = OptionType.Create("Color").Value;
        _fixture.Context.Set<OptionType>().Add(ot);
        
        var ov = ot.AddValue("Red", "Bright Red").Value;
        _fixture.Context.Set<OptionValue>().Add(ov); // Explicitly track for test
        
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _fixture.Context.Set<OptionValue>()
            .Where(x => x.Id == ov.Id)
            .ProjectToType<OptionValueModel>()
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ov.Id);
        result.Name.Should().Be("Red");
        result.Presentation.Should().Be("Bright Red");
    }
}