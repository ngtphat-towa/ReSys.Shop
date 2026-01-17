using Microsoft.EntityFrameworkCore;


using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValuePositions;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.OptionTypes.OptionValues;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionValues")]
public class UpdateOptionValuePositionsTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "Handle: Should update multiple positions successfully")]
    public async Task Handle_ValidRequest_ShouldUpdatePositions()
    {
        // Arrange
        var ot = OptionType.Create("Sort").Value;
        var ov1 = ot.AddValue("A").Value;
        var ov2 = ot.AddValue("B").Value;
        fixture.Context.Set<OptionType>().Add(ot);
        fixture.Context.Set<OptionValue>().AddRange(ov1, ov2); // Explicitly track

        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateOptionValuePositions.Handler(fixture.Context);
        var request = new UpdateOptionValuePositions.Request(
            new[]
            {
                new UpdateOptionValuePositions.ValuePosition(ov1.Id, 10),
                new UpdateOptionValuePositions.ValuePosition(ov2.Id, 20)
            });
        var command = new UpdateOptionValuePositions.Command(ot.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        var updatedOv1 = await fixture.Context.Set<OptionValue>().FirstAsync(x => x.Id == ov1.Id, TestContext.Current.CancellationToken);
        var updatedOv2 = await fixture.Context.Set<OptionValue>().FirstAsync(x => x.Id == ov2.Id, TestContext.Current.CancellationToken);

        updatedOv1.Position.Should().Be(10);
        updatedOv2.Position.Should().Be(20);
    }
}