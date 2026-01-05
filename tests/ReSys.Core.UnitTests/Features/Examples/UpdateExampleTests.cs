using ErrorOr;
using FluentAssertions;
using NSubstitute;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Examples.UpdateExample;
using ReSys.Core.Features.Examples.Common;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Common.Storage;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Examples;

public class UpdateExampleTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly UpdateExample.Handler _handler;

    public UpdateExampleTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _fileService = Substitute.For<IFileService>();
        _handler = new UpdateExample.Handler(_context, _fileService);
    }

    [Fact(DisplayName = "Handle: Should successfully update an existing example with new details when the request is valid")]
    public async Task Handle_ValidRequest_ShouldUpdateExample()
    {
        // Arrange
        var exampleId = Guid.NewGuid();
        var initialName = $"OldName_{Guid.NewGuid()}";
        var example = new Example 
        {
            Id = exampleId, 
            Name = initialName, 
            Description = "Old Desc",
            Price = 10,
            ImageUrl = "old.jpg",
            Status = ExampleStatus.Draft,
            HexColor = "#000000"
        };
        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var newName = $"NewName_{Guid.NewGuid()}";
        var request = new UpdateExample.Request
        {
            Name = newName,
            Description = "New Desc",
            Price = 20,
            ImageUrl = "new.jpg",
            Status = ExampleStatus.Active,
            HexColor = "#FFFFFF"
        };
        var command = new UpdateExample.Command(exampleId, request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse("because the request is valid and name is changed to something unique");
        result.Value.Name.Should().Be(newName);
        result.Value.Price.Should().Be(20);
        result.Value.Status.Should().Be(ExampleStatus.Active);
        result.Value.HexColor.Should().Be("#FFFFFF");

        var dbExample = await _context.Set<Example>().FindAsync(new object?[] { exampleId, TestContext.Current.CancellationToken }, TestContext.Current.CancellationToken);
        dbExample!.Name.Should().Be(newName);
        dbExample.Status.Should().Be(ExampleStatus.Active);
        dbExample.HexColor.Should().Be("#FFFFFF");
    }

    [Fact(DisplayName = "Handle: Should return a conflict error when updating an example name to one that already exists")]
    public async Task Handle_NameConflict_ShouldReturnConflict()
    {
        // Arrange
        var existingName = $"Conflict_{Guid.NewGuid()}";
        var example1Id = Guid.NewGuid();
        var example2Id = Guid.NewGuid();
        
        _context.Set<Example>().AddRange(
            new Example { Id = example1Id, Name = existingName, Description = "D1", Price = 10 },
            new Example { Id = example2Id, Name = "UniqueName", Description = "D2", Price = 20 }
        );
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateExample.Request { Name = existingName, Description = "Changed", Price = 20 };
        var command = new UpdateExample.Command(example2Id, request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue("because the name already belongs to another example");
        result.FirstError.Should().BeEquivalentTo(ExampleErrors.DuplicateName);
    }

    [Fact(DisplayName = "Handle: Should return a not found error when attempting to update an example that does not exist")]
    public async Task Handle_NonExistentExample_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateExample.Request { Name = "Valid", Description = "Desc", Price = 10 };
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateExample.Command(nonExistentId, request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue("because the example ID does not exist");
        result.FirstError.Should().BeEquivalentTo(ExampleErrors.NotFound(nonExistentId));
    }
}

