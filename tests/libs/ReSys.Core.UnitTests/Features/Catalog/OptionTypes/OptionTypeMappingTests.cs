using Microsoft.EntityFrameworkCore;

using Mapster;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.Common;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
public class OptionTypeMappingTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public OptionTypeMappingTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;

        // Initialize Mapster for tests using the registration class we created
        new OptionTypeMappings().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact(DisplayName = "Projection: OptionTypeSelectListItem should map correctly")]
    public async Task Projection_OptionTypeSelectListItem_ShouldMapCorrectly()
    {
        // Arrange
        var optionType = OptionType.Create("Color", "Select Color").Value;
        _fixture.Context.Set<OptionType>().Add(optionType);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _fixture.Context.Set<OptionType>()
            .Where(x => x.Id == optionType.Id)
            .ProjectToType<OptionTypeSelectListItem>()
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(optionType.Id);
        result.Name.Should().Be(optionType.Name);
    }

    [Fact(DisplayName = "Projection: OptionTypeListItem should map correctly")]
    public async Task Projection_OptionTypeListItem_ShouldMapCorrectly()
    {
        // Arrange
        var optionType = OptionType.Create("Size", "Select Size").Value;
        optionType.Position = 5;
        optionType.Filterable = true;
        optionType.PublicMetadata["key"] = "value";

        _fixture.Context.Set<OptionType>().Add(optionType);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _fixture.Context.Set<OptionType>()
            .Where(x => x.Id == optionType.Id)
            .ProjectToType<OptionTypeListItem>()
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(optionType.Id);
        result.Name.Should().Be("Size");
        result.Presentation.Should().Be("Select Size");
        result.Position.Should().Be(5);
        result.Filterable.Should().BeTrue();
        result.PublicMetadata.Should().ContainKey("key");

        // Use ToString() to compare values as the DB provider might return JsonElement
        result.PublicMetadata["key"]!.ToString().Should().Be("value");
    }

    [Fact(DisplayName = "Projection: OptionTypeDetail should map correctly with values")]
    public async Task Projection_OptionTypeDetail_ShouldMapCorrectly()
    {
        // Arrange
        var optionType = OptionType.Create("Material").Value;
        optionType.AddValue("Cotton", "Cotton Material");
        optionType.AddValue("Silk", "Silk Material");

        _fixture.Context.Set<OptionType>().Add(optionType);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _fixture.Context.Set<OptionType>()
            .Where(x => x.Id == optionType.Id)
            .ProjectToType<OptionTypeDetail>()
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(optionType.Id);
        result.Name.Should().Be("Material");
        result.OptionValues.Should().HaveCount(2);
        result.OptionValues.Should().Contain(v => v.Name == "Cotton" && v.Presentation == "Cotton Material" && v.Position == 0);
        result.OptionValues.Should().Contain(v => v.Name == "Silk" && v.Presentation == "Silk Material" && v.Position == 1);
    }

    [Fact(DisplayName = "Projection: Should handle null metadata by providing empty dictionaries")]
    public async Task Projection_ShouldHandleNullMetadata()
    {
        // Arrange
        var optionType = OptionType.Create("NullMeta").Value;

        _fixture.Context.Set<OptionType>().Add(optionType);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _fixture.Context.Set<OptionType>()
            .Where(x => x.Id == optionType.Id)
            .ProjectToType<OptionTypeListItem>()
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.PublicMetadata.Should().NotBeNull();
        result.PrivateMetadata.Should().NotBeNull();
    }
}
