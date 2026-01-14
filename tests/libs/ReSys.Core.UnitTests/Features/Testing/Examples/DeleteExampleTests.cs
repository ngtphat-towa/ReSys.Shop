using NSubstitute;

using ReSys.Core.Common.Data;
using ReSys.Core.Features.Testing.Examples.DeleteExample;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Common.Storage;
using ReSys.Core.Domain.Testing.Examples;

namespace ReSys.Core.UnitTests.Features.Testing.Examples;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Examples")]
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
    public async Task Handle_ExampleExists_DeletesExample()
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
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new DeleteExample.Command(exampleId);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse("because the example exists and can be deleted");
        result.Value.Should().Be(ErrorOr.Result.Deleted);

        var dbExample = await _context.Set<Example>().FindAsync([exampleId], TestContext.Current.CancellationToken);
        dbExample.Should().BeNull("because the example was removed from the database");
    }

    [Fact(DisplayName = "Handle: Should return a not found error when attempting to delete an example that does not exist")]
    public async Task Handle_ExampleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteExample.Command(nonExistentId);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue("because the example ID does not exist in the database");
        result.FirstError.Should().BeEquivalentTo(ExampleErrors.NotFound(nonExistentId));
    }
}


