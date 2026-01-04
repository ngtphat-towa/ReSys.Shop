using ErrorOr;
using FluentAssertions;
using NSubstitute;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Examples.DeleteExample;
using ReSys.Core.Features.Examples.Common;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Common.Storage;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Examples;

public class DeleteExampleTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly DeleteExample.Handler _handler;

    public DeleteExampleTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _fileService = Substitute.For<IFileService>();
        _handler = new DeleteExample.Handler(_context, _fileService);
    }

    [Fact(DisplayName = "Handle: Should successfully delete an existing example from the database")]
    public async Task Handle_ExistingExample_ShouldDelete()
    {
        // Arrange
        var exampleId = Guid.NewGuid();
        var example = new Example 
        {
            Id = exampleId, 
            Name = $"DeleteMe_{Guid.NewGuid()}", 
            Description = "Desc",
            Price = 10 
        };
        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteExample.Command(exampleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the example exists and can be deleted");
        result.Value.Should().Be(ErrorOr.Result.Deleted);

        var dbExample = await _context.Set<Example>().FindAsync(exampleId);
        dbExample.Should().BeNull("because the example was removed from the database");
    }

    [Fact(DisplayName = "Handle: Should return a not found error when attempting to delete an example that does not exist")]
    public async Task Handle_NonExistentExample_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteExample.Command(nonExistentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue("because the example ID does not exist in the database");
        result.FirstError.Should().BeEquivalentTo(ExampleErrors.NotFound(nonExistentId));
    }
}
