using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Features.Examples.Common;
using ReSys.Core.Features.Examples.GetExampleById;
using ReSys.Core.Entities;
using ReSys.Core.Interfaces;

namespace ReSys.Core.UnitTests.Features.Examples;

public class GetExampleByIdTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly GetExampleById.Handler _handler;

    public GetExampleByIdTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new GetExampleById.Handler(_context);
    }

    [Fact(DisplayName = "Should return the correct example details when an example with the specified ID exists")]
    public async Task Handle_ExistingExample_ShouldReturnDetails()
    {
        // Arrange
        var exampleId = Guid.NewGuid();
        var example = new Example 
        {
            Id = exampleId,
            Name = $"TestExample_{Guid.NewGuid()}",
            Description = "Full Description",
            Price = 49.99m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new GetExampleById.Request(exampleId);
        var query = new GetExampleById.Query(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the example exists");
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(exampleId);
        result.Value.Name.Should().Be(example.Name);
    }

    [Fact(DisplayName = "Should return a not found error when searching for an example ID that does not exist")]
    public async Task Handle_NonExistentExample_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new GetExampleById.Request(nonExistentId);
        var query = new GetExampleById.Query(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue("because no example exists with the given ID");
        result.FirstError.Should().BeEquivalentTo(ExampleErrors.NotFound(nonExistentId));
    }
}

