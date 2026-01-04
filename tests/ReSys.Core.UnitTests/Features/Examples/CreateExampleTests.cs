using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Features.Examples.Common;
using ReSys.Core.Features.Examples.CreateExample;
using ReSys.Core.Domain;
using ReSys.Core.Common.Data;

namespace ReSys.Core.UnitTests.Features.Examples;

public class CreateExampleTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly CreateExample.Handler _handler;

    public CreateExampleTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new CreateExample.Handler(_context);
    }

    [Fact(DisplayName = "Should successfully create an example and save it to the database when the request is valid")]
    public async Task Handle_ValidRequest_ShouldCreateExample()
    {
        // Arrange
        var uniqueName = $"Example_{Guid.NewGuid()}";
        var request = new CreateExample.Request
        {
            Name = uniqueName,
            Description = "A valid example description",
            Price = 99.99m,
            ImageUrl = "http://example.com/image.jpg"
        };
        var command = new CreateExample.Command(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the request is valid and the name is unique");
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(uniqueName);

        var dbExample = await _context.Set<Example>().FindAsync(result.Value.Id);
        dbExample.Should().NotBeNull("because the example should be persisted in the database");
    }

    [Fact(DisplayName = "Should return a conflict error when attempting to create an example with a name that already exists")]
    public async Task Handle_DuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var duplicateName = $"Duplicate_{Guid.NewGuid()}";
        _context.Set<Example>().Add(new Example { Id = Guid.NewGuid(), Name = duplicateName, Description = "Existing", Price = 10 });
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new CreateExample.Request { Name = duplicateName, Description = "New", Price = 20 };
        var command = new CreateExample.Command(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue("because an example with the same name already exists");
        result.FirstError.Should().BeEquivalentTo(ExampleErrors.DuplicateName);
    }
}
